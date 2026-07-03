using MediatR;
using Microsoft.Extensions.Logging;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Scheduling;
using Viora.Application.Billings;
using Viora.Application.Wallets.ProvisionRecharge;
using Viora.Domain.Abstractions;
using Viora.Domain.Billings.Invoices;
using Viora.Domain.Orders;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Plans;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons.Event;
using Viora.Domain.Subscriptions.Events;

namespace Viora.Application.Payments.Webhooks;

internal sealed class HandleKashierWebhookCommandHandler(
    IPaymentService paymentService,
    ISubscriptionOrderRepository subscriptionOrderRepository,
    IAddonOrderRepository addonOrderRepository,
    IInvoiceRepository invoiceRepository,
    ISubscriptionRepository subscriptionRepository,
    IPlanRepository planRepository,
    IDomainEventScheduler scheduler,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ISender sender,
    ILogger<HandleKashierWebhookCommandHandler> logger) : ICommandHandler<HandleKashierWebhookCommand>
{
    private const string SuccessStatus = "SUCCESS";

    public async Task<Result> Handle(HandleKashierWebhookCommand request, CancellationToken cancellationToken)
    {
        var parsed = KashierWebhookParser.Parse(request.RawBody);
        if (parsed.IsFailure)
        {
            logger.LogWarning("Kashier {Kind} webhook: unparseable body.", request.Kind);
            return Result.Success(); // swallow -> 200, nothing to act on
        }

        var data = parsed.Value.Payload.Data;

        // 1. Signature: the only outcome that is NOT 200.
        var verify = paymentService.VerifySignature(parsed.Value.SignatureFields, request.SignatureHeader);
        if (verify.IsFailure)
        {
            logger.LogWarning("Kashier {Kind} webhook: signature verification failed.", request.Kind);
            return verify; // Unauthorized -> 401
        }

        // 2. Recharge has no order/invoice — the merchant reference carries the user id. On success,
        //    credit the wallet (idempotent via the gateway transaction id); everything returns 200.
        if (request.Kind == WebhookKind.Recharge)
            return await HandleRechargeAsync(data, cancellationToken);

        // 3. Resolve the order via the merchant reference we set as the order id.
        if (!Guid.TryParse(data.MerchantOrderId, out var orderId))
        {
            logger.LogWarning("Kashier {Kind} webhook: invalid merchant order id '{Id}'.", request.Kind, data.MerchantOrderId);
            return Result.Success();
        }

        Order? order = request.Kind == WebhookKind.Subscription
            ? await subscriptionOrderRepository.GetByIdAsync(orderId, cancellationToken)
            : await addonOrderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Kashier {Kind} webhook: unknown order {OrderId}.", request.Kind, orderId);
            return Result.Success();
        }

        // 3. Idempotency: a replayed webhook for an already-paid order is a no-op.
        if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Fullfiled)
        {
            logger.LogInformation("Kashier {Kind} webhook: order {OrderId} already paid; ignoring duplicate.", request.Kind, orderId);
            return Result.Success();
        }

        // 4. Failed payment -> mark the order failed (keep the invoice for a retry).
        if (!string.Equals(data.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase))
        {
            var failed = order.MarkFailed();
            if (failed.IsFailure)
                logger.LogWarning("Kashier {Kind} webhook: could not mark order {OrderId} failed: {Error}.", request.Kind, orderId, failed.Error.Name);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        // 5. Success: load the invoice and verify the amount before confirming anything.
        if (order.InvoiceId is null)
        {
            logger.LogError("Kashier {Kind} webhook: paid order {OrderId} has no invoice attached.", request.Kind, orderId);
            return Result.Success();
        }

        var invoice = await invoiceRepository.GetByIdAsync(order.InvoiceId.Value, cancellationToken);
        if (invoice is null)
        {
            logger.LogError("Kashier {Kind} webhook: invoice {InvoiceId} for order {OrderId} not found.", request.Kind, order.InvoiceId, orderId);
            return Result.Success();
        }

        if (data.Amount != invoice.Total.Amount
            || !string.Equals(data.Currency, invoice.Currency.Code, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogError(
                "Kashier {Kind} webhook: amount/currency mismatch for order {OrderId}. Webhook {WebhookAmount} {WebhookCurrency} vs invoice {InvoiceAmount} {InvoiceCurrency}. NOT confirming.",
                request.Kind, orderId, data.Amount, data.Currency, invoice.Total.Amount, invoice.Currency.Code);
            return Result.Success(); // never auto-confirm a mismatched amount
        }

        var invoicePaid = invoice.MarkPaid();
        if (invoicePaid.IsFailure)
        {
            logger.LogError("Kashier {Kind} webhook: invoice {InvoiceId} cannot be marked paid: {Error}.", request.Kind, invoice.Id, invoicePaid.Error.Name);
            return Result.Success();
        }

        var orderPaid = order.MarkPaid();
        if (orderPaid.IsFailure)
        {
            logger.LogWarning("Kashier {Kind} webhook: order {OrderId} cannot be marked paid: {Error}.", request.Kind, orderId, orderPaid.Error.Name);
            return Result.Success();
        }

        // 6. Enqueue provisioning on the outbox (immediate); dispatcher runs it with retries.
        var scheduled = await ScheduleProvisioningAsync(order, cancellationToken);
        if (scheduled.IsFailure)
        {
            logger.LogError("Kashier {Kind} webhook: provisioning could not be scheduled for order {OrderId}: {Error}.", request.Kind, orderId, scheduled.Error.Name);
            return Result.Success();
        }

        // Paid state + scheduled provisioning commit atomically.
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Kashier {Kind} webhook: order {OrderId} marked paid; provisioning scheduled.", request.Kind, orderId);
        return Result.Success();
    }

    private async Task<Result> HandleRechargeAsync(PaymentData data, CancellationToken cancellationToken)
    {
        if (!string.Equals(data.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Kashier recharge webhook: non-success status {Status} for ref {Reference}; ignoring.", data.Status, data.TransactionId);
            return Result.Success();
        }

        // The merchant reference carries the user id (set at session creation).
        if (!Guid.TryParse(data.MerchantOrderId, out var userId))
        {
            logger.LogWarning("Kashier recharge webhook: invalid user reference '{Reference}'.", data.MerchantOrderId);
            return Result.Success();
        }

        // Idempotent credit (dedup on the gateway transaction id). Handler swallows/loggs anomalies -> 200.
        return await sender.Send(new ProvisionRechargeCommand(userId, data.Amount, data.Currency, data.TransactionId), cancellationToken);
    }

    private async Task<Result> ScheduleProvisioningAsync(Order order, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        switch (order)
        {
            case SubscriptionOrder subscriptionOrder:
                return await ScheduleSubscriptionProvisioningAsync(subscriptionOrder, now, cancellationToken);

            case AddonOrder addonOrder:
                if (addonOrder.SubscriptionId is null)
                    return Result.Failure(OrderError.InvalidStatusTransition);

                var addonIds = addonOrder.Addons.Select(addon => addon.Id).ToList();
                await scheduler.ScheduleAsync(
                    new AddonAddedDomainEvent(addonOrder.SubscriptionId.Value, addonIds),
                    now, cancellationToken);
                return Result.Success();

            default:
                return Result.Failure(OrderError.InvalidSubscriptionOrderType);
        }
    }

    private async Task<Result> ScheduleSubscriptionProvisioningAsync(SubscriptionOrder order, DateTime now, CancellationToken cancellationToken)
    {
        if (order.SubscriptionOrderType == SubscriptionOrderType.NewSubscription)
        {
            await scheduler.ScheduleAsync(
                new SubscriptionCreatedDomainEvent(order.PlanId, order.OrganizationId),
                now, cancellationToken);
            return Result.Success();
        }

        if (order.SubscriptionId is null)
            return Result.Failure(OrderError.InvalidStatusTransition);

        if (order.SubscriptionOrderType == SubscriptionOrderType.Renewal)
        {
            await scheduler.ScheduleAsync(
                new SubscriptionRenewedDomainEvent(order.SubscriptionId.Value, order.PlanId, order.OrganizationId),
                now, cancellationToken);
            return Result.Success();
        }

        if (order.SubscriptionOrderType == SubscriptionOrderType.ChangeSubscription)
        {
            // Old plan id + new period are derived here from the subscription and the new plan.
            var subscription = await subscriptionRepository.GetByIdWithAddonAsync(order.SubscriptionId.Value, cancellationToken);
            if (subscription is null)
                return Result.Failure(OrderError.InvalidStatusTransition);

            var newPlan = await planRepository.GetByIdAsync(order.PlanId, cancellationToken);
            if (newPlan is null)
                return Result.Failure(OrderError.InvalidStatusTransition);

            var endTime = newPlan.PlanPeriod.CalculateEndTime(now);
            if (endTime.IsFailure)
                return Result.Failure(endTime.Error);

            await scheduler.ScheduleAsync(
                new SubscriptionPlanChangedDomainEvent(
                    subscription.Id,
                    subscription.PlanId,
                    order.PlanId,
                    order.OrganizationId,
                    now,
                    endTime.Value),
                now, cancellationToken);
            return Result.Success();
        }

        return Result.Failure(OrderError.InvalidSubscriptionOrderType);
    }
}
