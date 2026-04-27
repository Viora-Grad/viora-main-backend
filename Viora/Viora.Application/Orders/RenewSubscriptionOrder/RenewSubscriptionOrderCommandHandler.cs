using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Organizations;
using Viora.Domain.Plans;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons;

namespace Viora.Application.Orders.RenewSubscriptionOrder;

public class RenewSubscriptionOrderCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IDateTimeProvider dateTimeProvider,
    IOrganizationRepository organizationRepository,
    IPlanRepository planRepository,
    ISubscriptionOrderRepository subscriptionOrderRepository,
    ILimitedFeatureAddonRepository limitedFeatureAddonRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RenewSubscriptionOrderCommand>
{
    public async Task<Result> Handle(RenewSubscriptionOrderCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdWithAddonAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription with id {request.SubscriptionId} not found.");
        var organization = await organizationRepository.GetByIdAsync(subscription.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with id {subscription.OrganizationId} not found.");
        var plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken)
            ?? throw new NotFoundException($"Plan with id {subscription.PlanId} not found.");

        var limitedFeatureAddonIds = subscription.Addons.Select(x => x.LimitedFeatureAddonId).ToList();
        var limitedFeatureAddon = await limitedFeatureAddonRepository.GetByIdsAsync(limitedFeatureAddonIds, cancellationToken);

        var totalPrice = plan.Price + limitedFeatureAddon.Sum(x => x.Price);

        var subscriptionOrder = SubscriptionOrder.CreateRenewSubscriptionOrder(
            organization.Id,
            plan.Id,
            request.SubscriptionId,
            totalPrice,
            dateTimeProvider.UtcNow);

        if (subscriptionOrder.IsFailure)
            return Result.Failure(subscriptionOrder.Error);
        subscriptionOrderRepository.Add(subscriptionOrder.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
