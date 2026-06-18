using MediatR;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Events;

namespace Viora.Application.Subscriptions.RenewSubscriptions;

/// <summary>
/// Domain event triggered when a subscription is  renewed.
/// 
/// Responsibilities:
/// - Initializes or updates feature usage records for the subscription.
/// - Ensures feature limits are correctly applied for the subscription period.
/// 
/// </summary>

internal class SubscriptionRenewedDomainEventHandler(
    IFeatureUsageRepository featureUsageRepository,
    ISubscriptionRepository subscriptionRepository,
    IPlanRepository planRepository,
    IDateTimeProvider dateTimeProvider,
    IOrganizationRepository organizationRepository,
    ILimitedFeatureAddonRepository limitedFeatureAddonRepository
    ) : INotificationHandler<SubscriptionRenewedDomainEvent>
{
    public async Task Handle(SubscriptionRenewedDomainEvent notification, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdWithAddonAsync(notification.SubscriptionId, cancellationToken)
           ?? throw new NotFoundException($"Subscription with id {notification.SubscriptionId} not found.");

        var plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken)
            ?? throw new NotFoundException($"the plan with id {subscription.PlanId} not found");

        var organization = await organizationRepository.GetByIdAsync(notification.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {notification.OrganizationId} not found");

        var startDate = dateTimeProvider.UtcNow;
        var endDate = plan.PlanPeriod.CalculateEndTime(startDate);

        if (endDate.IsFailure)
            throw new InvalidOperationException($"Failed to calculate end date for subscription renewal: {endDate.Error}");

        var result = subscription.Renew(startDate, endDate.Value);

        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to renew subscription: {result.Error}");

        await RenewFeatureUsage(
            subscription,
            plan.Id,
            organization.Id,
            startDate, endDate.Value, cancellationToken);

        subscriptionRepository.Add(result.Value);
    }

    private async Task RenewFeatureUsage(
        Subscription subscription,
         Guid PlanId,
         Guid organizationId,
         DateTime startDate,
         DateTime endDate,
         CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(PlanId, cancellationToken)
            ?? throw new NotFoundException($"the plan with id {PlanId} not found");

        var Addons = subscription.GetAddons();
        var AddonIds = Addons.Where(x => x.IsActive == true)
            .Select(x => x.LimitedFeatureAddonId)
            .ToList();
        var LimitedFeatureAddon = await limitedFeatureAddonRepository.GetByIdsAsync(AddonIds, cancellationToken);

        var planlimitedFeatures = plan.PlanLimitedFeatures;

        foreach (var planlimitedFeature in planlimitedFeatures)
        {
            var limitedFeature = planlimitedFeature.LimitedFeature;
            var featureUsage = await featureUsageRepository.GetByOrganizationIdAndFeatureIdAsync(
                 organizationId,
                 limitedFeature.Id, cancellationToken)
                 ?? throw new NotFoundException(
                     $"The feature usage for organization with id {organizationId}" +
                     $" and limited feature with id {limitedFeature.Id} was not found");

            var featureAddons = LimitedFeatureAddon.FindAll(x => x.LimitedFeatureId == limitedFeature.Id);

            if (featureAddons == null || !featureAddons.Any())
                featureUsage.Renew(planlimitedFeature.LimitValue, startDate, endDate);
            else
            {
                var newLimit = planlimitedFeature.LimitValue + featureAddons.Sum(x => x.RestoreValue);
                featureUsage.Renew(newLimit, startDate, endDate);
            }
        }
    }
}
