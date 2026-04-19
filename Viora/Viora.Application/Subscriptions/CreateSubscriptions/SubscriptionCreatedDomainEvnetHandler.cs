using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions.Events;

namespace Viora.Application.Subscriptions.CreateSubscriptions;

/// <summary>
/// Domain event triggered when a subscription is created.
/// 
/// Responsibilities:
/// - Creates usage entries for limited features based on plan configuration.
/// - Ensures feature limits are correctly applied for the subscription period.
/// 
/// </summary>

internal class SubscriptionCreatedDomainEvnetHandler(
    IFeatureUsageRepository featureUsageRepository,
    IPlanFeatureRepository planFeatureRepository,
    ILimitedFeatureRepository limitedFeatureRepository,
    IUnitOfWork unitOfWork
    ) : INotificationHandler<SubscriptionCreatedDomainEvent>
{
    public async Task Handle(SubscriptionCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var features = await planFeatureRepository.GetByPlanIdAsync(notification.PlanId, cancellationToken)
            ?? throw new NotFoundException($"Plan features with {notification.PlanId} not found.");
        var limitedFeaturesIds = features.Select(feature => feature.LimitedFeatureId).ToList();
        var limitedFeatures = await limitedFeatureRepository.GetByIdsAsync(limitedFeaturesIds, cancellationToken);
        var featureUsages = FeatureUsage.CreateMany(
            notification.OrganizationId,
            limitedFeatures,
            notification.SubscriptionsStartTime,
            notification.SubscriptionEndTime
        );

        if (featureUsages.IsFailure)
            throw new InvalidOperationException("Failed to create feature usages for the subscription.");

        featureUsageRepository.AddRange(featureUsages.Value);
        await unitOfWork.SaveChangesAsync();

    }
}
