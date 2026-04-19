using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
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
    IPlanFeatureRepository planFeatureRepository,
    ILimitedFeatureRepository limitedFeatureRepository,
    IFeatureUsageRepository featureUsageRepository,
    IUnitOfWork unitOfWork
    ) : INotificationHandler<SubscriptionRenewedDomainEvent>
{
    public async Task Handle(SubscriptionRenewedDomainEvent notification, CancellationToken cancellationToken)
    {
        var Features = await planFeatureRepository.GetByPlanIdAsync(notification.PlanId, cancellationToken)
            ?? throw new NotFoundException($"The plan features for plan with id {notification.PlanId} were not found");
        foreach (var feature in Features)
        {
            var LimitedFeature = await limitedFeatureRepository.GetByIdAsync(feature.LimitedFeatureId, cancellationToken)
                ?? throw new NotFoundException($"The limited feature for plan feature with id {feature.LimitedFeatureId} was not found");

            var featureUsage = await featureUsageRepository.GetByOrganizationIdAndFeatureIdAsync(
                notification.OrganizationId,
                LimitedFeature.Id, cancellationToken)
                ?? throw new NotFoundException(
                    $"The feature usage for organization with id {notification.OrganizationId}" +
                    $" and limited feature with id {LimitedFeature.Id} was not found");
            featureUsage.Renew(LimitedFeature.Limit, notification.EndDate, notification.StartDate);
        }
        await unitOfWork.SaveChangesAsync();
    }
}
