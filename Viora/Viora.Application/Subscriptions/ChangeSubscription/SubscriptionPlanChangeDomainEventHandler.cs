using MediatR;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Events;

namespace Viora.Application.Subscriptions.ChangeSubscription;

public class SubscriptionPlanChangeDomainEventHandler(
    IPlanFeatureRepository planFeatureRepository,
    IPlanRepository planRepository,
    IFeatureUsageRepository featureUsageRepository,
    ILimitedFeatureRepository limitedFeatureRepository,
    ISubscriptionRepository subscriptionRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : INotificationHandler<SubscriptionPlanChangedDomainEvent>
{

    public async Task Handle(SubscriptionPlanChangedDomainEvent notification, CancellationToken cancellationToken)
    {

        var newPlan = await planRepository.GetByIdAsync(notification.NewPlanId, cancellationToken)
             ?? throw new NotFoundException($"the plan with id{notification.NewPlanId} not found");

        var subscription = await subscriptionRepository.GetByIdAsync(notification.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"the subscription with id {notification.SubscriptionId} not found");

        var startTime = dateTimeProvider.UtcNow;
        var endTimeResult = newPlan.PlanPeriod.CalculateEndTime(startTime);

        if (endTimeResult.IsFailure)
            throw new InvalidOperationException($"Failed to calculate end time for the new plan: {endTimeResult.Error}");

        var result = subscription.ChangePlan(subscription.OrganizationId, subscription.PlanId, newPlan.Id, startTime, endTimeResult.Value);

        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to change subscription plan: {result.Error}");

        subscriptionRepository.Add(result.Value);
        await ChangeFeatureUsage(notification.OldPlanId, newPlan.Id, subscription.OrganizationId, startTime, endTimeResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeFeatureUsage(Guid oldPlanId, Guid newPlanId, Guid organizationId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        //remove all feature usages for the old plan
        var oldPlan = await planRepository.GetByIdAsync(oldPlanId, cancellationToken)
            ?? throw new NotFoundException($"Plan with ID {oldPlanId} not found.");

        var oldPlanFeatures = await planFeatureRepository.GetByPlanIdAsync(oldPlanId, cancellationToken)
            ?? throw new NotFoundException($"Features for Plan with ID {oldPlanId} not found.");

        var LimitedFeaturesIds = oldPlanFeatures.Where(f => f.LimitedFeatureId.HasValue).Select(f => f.LimitedFeatureId.Value).ToList();
        featureUsageRepository.RemoveRangeByLimitedIdAndOrganizationId(LimitedFeaturesIds, organizationId);

        //add feature usages for the new plan
        var newPlanFeatures = await planFeatureRepository.GetByPlanIdAsync(newPlanId, cancellationToken)
            ?? throw new NotFoundException($"Features for Plan with ID {newPlanId} not found.");

        var newLimitedFeaturesIds = newPlanFeatures.Where(f => f.LimitedFeatureId.HasValue).Select(f => f.LimitedFeatureId.Value).ToList();

        var newLimitedFeatures = await limitedFeatureRepository.GetByIdsAsync(newLimitedFeaturesIds, cancellationToken)
            ?? throw new NotFoundException($"Limited Features with IDs {string.Join(", ", newLimitedFeaturesIds)} not found.");

        var newFeatureUsages = FeatureUsage.CreateMany(organizationId, newLimitedFeatures, startTime, endTime);

        if (newFeatureUsages.IsFailure)
            throw new InvalidOperationException($"Failed to create feature usages for the new plan: {newFeatureUsages.Error}");

        featureUsageRepository.AddRange(newFeatureUsages.Value);
    }
}
