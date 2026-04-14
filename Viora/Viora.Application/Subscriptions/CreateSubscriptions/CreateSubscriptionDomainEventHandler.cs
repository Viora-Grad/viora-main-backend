using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions.Events;

namespace Viora.Application.Subscriptions.CreateSubscriptions;

internal class CreateSubscriptionDomainEventHandler(
    IFeatureUsageRepository featureUsageRepository,
    IPlanFeatureRepository planFeatureRepository,
    ILimitedFeatureRepository limitedFeatureRepository,
    IUnitOfWork unitOfWork
    ) : INotificationHandler<CreateSubscriptionDomainEvent>
{
    public async Task Handle(CreateSubscriptionDomainEvent notification, CancellationToken cancellationToken)
    {
        var limitedFeatures = await planFeatureRepository.GetByPlanIdAsync(notification.PlanId, cancellationToken);
        if (limitedFeatures != null)
        {
            foreach (var feature in limitedFeatures)
            {
                var limitedFeature = await limitedFeatureRepository.GetByIdAsync(feature.LimitedFeatureId, cancellationToken);
                var limitedFeatureUsage = FeatureUsage.Create(
                    notification.OrganizationId,
                    feature.FeatureId,
                    limitedFeature.Limit,
                    notification.SubscriptionsStartTime,
                    notification.SubscriptionEndTime
                );
                if (limitedFeatureUsage.IsFailure)
                    throw new InvalidInputException("feature limit must be greater than or equal to 0");
                featureUsageRepository.Add(limitedFeatureUsage.Value);
            }
        }
        await unitOfWork.SaveChangesAsync();

    }
}
