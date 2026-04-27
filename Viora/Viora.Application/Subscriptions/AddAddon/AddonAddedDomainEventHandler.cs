using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations;
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
    ILimitedFeatureRepository limitedFeatureRepository,
    IUnitOfWork unitOfWork) : INotificationHandler<AddonAddedDomainEvent>
{
    public async Task Handle(AddonAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdWithAddonAsync(notification.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription with id {notification.SubscriptionId} not found.");

        var addons = await limitedFeatureAddonRepository.GetByIdsAsync(notification.AddonIds, cancellationToken);
        if (addons is null || !addons.Any())
            throw new NotFoundException($"Addons with ids {string.Join(", ", notification.AddonIds)} not found.");

        var organization = await organizationRepository.GetByIdAsync(subscription.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with id {subscription.OrganizationId} not found.");

        var featureUsages = await featureUsageRepository.GetByOrganizationIdAsync(organization.Id, cancellationToken);
        if (featureUsages is null || !featureUsages.Any())
            throw new NotFoundException($"Feature usage for organization with id {organization.Id} not found.");

        var result = subscription.AddAddons(notification.AddonIds);

        if (result.IsFailure)
            throw new InvalidOperationException("Failed to add addons to subscription: " + result.Error);

        await AddFeatureAddonUsage(subscription, organization, featureUsages, addons, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
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

            if (limitedFeatureUsage != null)
            {
                limitedFeatureUsage.AddAddon(addon.RestoreValue);
                continue;
            }
            var limitedFeatureUsageResult = FeatureUsage.Create(organization.Id, limitedFeature, subscription.SubscriptionsStartTime, subscription.SubscriptionsEndTime);

            if (limitedFeatureUsageResult.IsFailure)
                throw new InvalidOperationException("Failed to create feature usage for addon: " + limitedFeatureUsageResult.Error);
            featureUsageRepository.Add(limitedFeatureUsageResult.Value);
        }
    }
}
