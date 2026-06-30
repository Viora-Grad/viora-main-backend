using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Event;

namespace Viora.Application.Subscriptions.AddAddon;

public class AddonAddedDomainEventHandler(
    ISubscriptionRepository subscriptionRepository,
    IOrganizationRepository organizationRepository,
    ILimitedFeatureAddonRepository limitedFeatureAddonRepository,
    IFeatureUsageRepository featureUsageRepository,
    IPlanLimitedFeatureRepository planLimitedFeatureRepository,
    ILimitedFeatureRepository limitedFeatureRepository
    ) : INotificationHandler<AddonAddedDomainEvent>
{
    public async Task Handle(AddonAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdWithAddonAsync(notification.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription with id {notification.SubscriptionId} not found.");

        // Idempotency: skip addons already attached to this subscription (duplicate dispatch).
        var alreadyAttached = subscription.Addons.Select(a => a.LimitedFeatureAddonId).ToHashSet();
        var newAddonIds = notification.AddonIds.Where(id => !alreadyAttached.Contains(id)).ToList();
        if (newAddonIds.Count == 0)
            return;

        var addons = await limitedFeatureAddonRepository.GetByIdsAsync(newAddonIds, cancellationToken);
        if (addons is null || !addons.Any())
            throw new NotFoundException($"Addons with ids {string.Join(", ", newAddonIds)} not found.");

        var organization = await organizationRepository.GetByIdAsync(subscription.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with id {subscription.OrganizationId} not found.");

        var featureUsages = await featureUsageRepository.GetByOrganizationIdAsync(organization.Id, cancellationToken);
        if (featureUsages is null || !featureUsages.Any())
            throw new NotFoundException($"Feature usage for organization with id {organization.Id} not found.");

        var result = subscription.AddAddons(newAddonIds);

        if (result.IsFailure)
            throw new InvalidOperationException("Failed to add addons to subscription: " + result.Error);

        await AddFeatureAddonUsage(subscription, organization, featureUsages, addons, cancellationToken);
    }

    private async Task AddFeatureAddonUsage(
        Subscription subscription,
        Organization organization,
        List<FeatureUsage> featureUsage,
        List<LimitedFeatureAddon> newaddons,
        CancellationToken cancellationToken)
    {
        foreach (var addon in newaddons)
        {
            var limitedFeatureUsage = featureUsage.FirstOrDefault(fu => fu.LimitedFeatureId == addon.LimitedFeatureId);
            var limitedFeature = await limitedFeatureRepository.GetByIdAsync(addon.LimitedFeatureId, cancellationToken)
                ?? throw new NotFoundException($"Limited feature with id {addon.LimitedFeatureId} not found.");
            var planLimitedFeature = await planLimitedFeatureRepository
                .GetPlanLimitedFeatureByLimitedFeatureIdAsync(subscription.PlanId, addon.LimitedFeatureId, cancellationToken)
                ?? throw new NotFoundException($"Plan limited feature with limited feature id {addon.LimitedFeatureId} not found.");

            if (limitedFeatureUsage != null)
            {
                limitedFeatureUsage.AddAddon(addon.RestoreValue);
                continue;
            }
            var limitedFeatureUsageResult = FeatureUsage.Create(
                organization.Id,
                limitedFeature.Id,
                subscription.SubscriptionsStartTime,
                subscription.SubscriptionsEndTime,
                planLimitedFeature.LimitValue);

            if (limitedFeatureUsageResult.IsFailure)
                throw new InvalidOperationException("Failed to create feature usage for addon: " + limitedFeatureUsageResult.Error);
            featureUsageRepository.Add(limitedFeatureUsageResult.Value);
        }
    }
}
