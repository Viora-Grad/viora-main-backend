using System.Globalization;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Billings;
using Viora.Domain.Abstractions;
using Viora.Domain.Billings;
using Viora.Domain.Billings.Invoices;
using Viora.Domain.Billings.Invoices.Internals;
using Viora.Domain.Orders;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;

namespace Viora.Application.Payments.CreatePaymentSession;

internal sealed class CreatePaymentSessionCommandHandler(
    ISubscriptionOrderRepository subscriptionOrderRepository,
    IAddonOrderRepository addonOrderRepository,
    IOrganizationRepository organizationRepository,
    IPlanRepository planRepository,
    ILimitedFeatureRepository limitedFeatureRepository,
    IInvoiceRepository invoiceRepository,
    IPaymentService paymentService,
    IPaymentSettings paymentSettings,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<CreatePaymentSessionCommand, CreatePaymentSessionResponse>
{
    public async Task<Result<CreatePaymentSessionResponse>> Handle(CreatePaymentSessionCommand request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        // Resolve the order: subscription-first, then addon (separate tables, no shared base table).
        var subscriptionOrder = await subscriptionOrderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        var addonOrder = subscriptionOrder is null
            ? await addonOrderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            : null;

        Order? order = (Order?)subscriptionOrder ?? addonOrder;
        if (order is null)
            return Result.Failure<CreatePaymentSessionResponse>(PaymentErrors.OrderNotFound);

        var organization = await organizationRepository.GetByIdAsync(order.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with id {order.OrganizationId} not found.");

        // Idempotent re-issue: a Pending order with an existing session just returns its URL.
        if (order.Status == OrderStatus.Pending && order.InvoiceId is not null)
        {
            var existingInvoice = await invoiceRepository.GetByIdAsync(order.InvoiceId.Value, cancellationToken);
            if (existingInvoice?.ExternalPayment is not null)
                return Result.Success(new CreatePaymentSessionResponse(existingInvoice.ExternalPayment.Url));
        }

        if (order.Status != OrderStatus.Draft)
            return Result.Failure<CreatePaymentSessionResponse>(PaymentErrors.OrderNotPayable);

        // Build invoice line items from the order, and pick the webhook segment by order type.
        List<InvoiceItemHolder> items;
        string webhookKind;

        if (subscriptionOrder is not null)
        {
            var plan = await planRepository.GetByIdAsync(subscriptionOrder.PlanId, cancellationToken)
                ?? throw new NotFoundException($"Plan with id {subscriptionOrder.PlanId} not found.");
            items = [new InvoiceItemHolder(plan.Name.value, plan.Description.Value, 1, plan.Price, 0m)];
            webhookKind = "subscription";
        }
        else
        {
            var addons = addonOrder!.Addons.ToList();
            var featureIds = addons.Select(a => a.LimitedFeatureId).Distinct().ToList();
            var features = await limitedFeatureRepository.GetByIdsAsync(featureIds, cancellationToken);
            var keyByFeatureId = features.ToDictionary(f => f.Id, f => f.Key.value);

            items = addons.Select(addon => new InvoiceItemHolder(
                keyByFeatureId.TryGetValue(addon.LimitedFeatureId, out var key) ? $"Add-on: {key}" : "Add-on",
                $"Add-on (+{addon.RestoreValue})",
                1,
                addon.Price,
                0m)).ToList();
            webhookKind = "addon";
        }

        var sequence = await invoiceRepository.NextSequenceAsync(cancellationToken);

        var invoiceResult = Invoice.Create(
            organization.Id,
            organization.Name,
            organization.BillingEmail,
            sequence,
            now,
            0m,
            items);
        if (invoiceResult.IsFailure)
            return Result.Failure<CreatePaymentSessionResponse>(invoiceResult.Error);

        var invoice = invoiceResult.Value;

        // Consistency guard: the issued total must match what the order recorded.
        if (invoice.Total.Amount != order.TotalPrice.Amount)
            return Result.Failure<CreatePaymentSessionResponse>(PaymentErrors.AmountMismatch);

        var issueResult = invoice.Issue(now.AddDays(1));
        if (issueResult.IsFailure)
            return Result.Failure<CreatePaymentSessionResponse>(issueResult.Error);

        invoiceRepository.Add(invoice);

        var attachResult = order.AttachInvoice(invoice.Id);
        if (attachResult.IsFailure)
            return Result.Failure<CreatePaymentSessionResponse>(attachResult.Error);

        var pendingResult = order.MarkPending();
        if (pendingResult.IsFailure)
            return Result.Failure<CreatePaymentSessionResponse>(pendingResult.Error);

        var paymentRequest = new PaymentRequest
        {
            Amount = invoice.Total.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            Currency = invoice.Currency.Code,
            Order = order.Id.ToString(),
            MerchantId = paymentSettings.MerchentId,
            ServerWebhook = $"{paymentSettings.PublicBaseUrl.TrimEnd('/')}/api/webhooks/kashier/{webhookKind}",
            MerchantRedirect = paymentSettings.ClientBaseUrl,
            Customer = new PaymentCustomer(organization.BillingEmail, organization.Id.ToString()),
            ExpireAt = now.AddMinutes(30),
            Description = $"Order {order.Id}",
        };

        var sessionResult = await paymentService.CreatePaymentSessionAsync(paymentRequest, cancellationToken);
        if (sessionResult.IsFailure)
            return Result.Failure<CreatePaymentSessionResponse>(sessionResult.Error);

        invoice.ExternalPayment = new ExternalPayment(sessionResult.Value.SessionId, sessionResult.Value.SessionUrl);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatePaymentSessionResponse(sessionResult.Value.SessionUrl));
    }
}
