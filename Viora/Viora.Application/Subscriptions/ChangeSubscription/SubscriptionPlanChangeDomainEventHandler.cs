using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions.Events;

namespace Viora.Application.Subscriptions.ChangeSubscription;

public class SubscriptionPlanChangeDomainEventHandler(
    IPlanFeatureRepository planFeatureRepository,
    IPlanRepository planRepository,
    IFeatureUsageRepository featureUsageRepository,
    ILimitedFeatureRepository limitedFeatureRepository,
    IUnitOfWork unitOfWork) : INotificationHandler<SubscriptionPlanChangedDomainEvent>
{

    public async Task Handle(SubscriptionPlanChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        //remove all feature usages for the old plan
        var oldPlan = await planRepository.GetByIdAsync(notification.OldPlanId, cancellationToken)
            ?? throw new NotFoundException($"Plan with ID {notification.OldPlanId} not found.");
        var oldPlanFeatures = await planFeatureRepository.GetByPlanIdAsync(notification.OldPlanId, cancellationToken)
            ?? throw new NotFoundException($"Features for Plan with ID {notification.OldPlanId} not found.");
        var LimitedFeaturesIds = oldPlanFeatures.Select(f => f.LimitedFeatureId).ToList();
        featureUsageRepository.RemoveRange(LimitedFeaturesIds);

        //add feature usages for the new plan
        var newPlan = await planRepository.GetByIdAsync(notification.NewPlanId, cancellationToken)
            ?? throw new NotFoundException($"Plan with ID {notification.NewPlanId} not found.");

        var newPlanFeatures = await planFeatureRepository.GetByPlanIdAsync(notification.NewPlanId, cancellationToken)
            ?? throw new NotFoundException($"Features for Plan with ID {notification.NewPlanId} not found.");

        var newLimitedFeaturesIds = newPlanFeatures.Select(f => f.LimitedFeatureId).ToList();

        var newLimitedFeatures = await limitedFeatureRepository.GetByIdsAsync(newLimitedFeaturesIds, cancellationToken)
            ?? throw new NotFoundException($"Limited Features with IDs {string.Join(", ", newLimitedFeaturesIds)} not found.");

        var newFeatureUsages = FeatureUsage.CreateMany(notification.OrganizationId, newLimitedFeatures, notification.StartTime, notification.EndTime);
        if (newFeatureUsages.IsFailure)
            throw new InvalidOperationException($"Failed to create feature usages for the new plan: {newFeatureUsages.Error}");
        featureUsageRepository.AddRange(newFeatureUsages.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
