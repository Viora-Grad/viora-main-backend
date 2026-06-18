using MediatR;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Organizations.OrganizationDetails;
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

internal class SubscriptionCreatedDomainEventHandler(
    IPlanRepository planRepository,
    IOrganizationRepository organizationRepository,
    ISubscriptionRepository subscriptionRepository,
    IFeatureUsageRepository featureUsageRepository,
    IDateTimeProvider dateTimeProvider
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
    }

    public async Task CreateFeaturesUsage(Guid planId, Guid organizationId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(planId, cancellationToken)
            ?? throw new NotFoundException($"the plan with id {planId} not found");
        var limitedFeatures = plan.PlanLimitedFeatures.Select(
            plf => plf.LimitedFeature
            ).ToList();

        var newFeatureUsages = plan.PlanLimitedFeatures.Select(plf =>
        FeatureUsage.Create(
            plf.LimitedFeatureId,
            organizationId,
            startDate,
            endDate,
            plf.LimitValue))
            .ToList();

        foreach (var newFeatureUsage in newFeatureUsages)
        {
            if (newFeatureUsage.IsFailure)
                throw new InvalidOperationException($"Failed to create feature usages for the new plan: {newFeatureUsage.Error}");
        }

        newFeatureUsages.ForEach(fu => featureUsageRepository.Add(fu.Value));
    }
}
