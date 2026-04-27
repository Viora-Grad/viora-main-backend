using MediatR;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;
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
    IPlanRepository planRepository,
    IOrganizationRepository organizationRepository,
    ISubscriptionRepository subscriptionRepository,
    IFeatureUsageRepository featureUsageRepository,
    IPlanFeatureRepository planFeatureRepository,
    ILimitedFeatureRepository limitedFeatureRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork
    ) : INotificationHandler<SubscriptionCreatedDomainEvent>
{
    public async Task Handle(SubscriptionCreatedDomainEvent notification, CancellationToken cancellationToken)
    {

        var plan = await planRepository.GetByIdAsync(notification.PlanId, cancellationToken)
           ?? throw new NotFoundException($"the plan with id {notification.PlanId} not found");
        var organization = await organizationRepository.GetByIdAsync(notification.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {notification.OrganizationId} not found");

        var startDate = dateTimeProvider.UtcNow;
        var endDate = plan.PlanPeriod.CalculateEndTime(startDate);

        if (endDate.IsFailure)
            throw new InvalidOperationException("Failed to calculate subscription end date based on the plan period.");
        if (endDate.Value < startDate)
            throw new InvalidOperationException("Invalid plan period: end date is before start date.");

        var result = Subscription.Create(
            notification.PlanId,
            notification.OrganizationId,
            startDate,
            endDate.Value);

        if (result.IsFailure)
            throw new InvalidOperationException("Failed to create subscription.");

        subscriptionRepository.Add(result.Value);
        await CreateFeaturesUsage(notification.PlanId, notification.OrganizationId, startDate, endDate.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync();

    }

    public async Task CreateFeaturesUsage(Guid planId, Guid organizationId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var features = await planFeatureRepository.GetByPlanIdAsync(planId, cancellationToken)
            ?? throw new NotFoundException($"Plan features with {planId} not found.");

        var limitedFeaturesIds = features.Where(f => f.LimitedFeatureId.HasValue).Select(f => f.LimitedFeatureId.Value).ToList();

        var limitedFeatures = await limitedFeatureRepository.GetByIdsAsync(limitedFeaturesIds, cancellationToken);

        var featureUsages = FeatureUsage.CreateMany(
            organizationId,
            limitedFeatures,
            startDate,
            endDate
        );

        if (featureUsages.IsFailure)
            throw new InvalidOperationException("Failed to create feature usages for the subscription.");

        featureUsageRepository.AddRange(featureUsages.Value);
    }
}
