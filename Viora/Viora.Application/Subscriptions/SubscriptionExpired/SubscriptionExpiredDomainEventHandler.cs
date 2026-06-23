using MediatR;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Scheduling;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Events;

namespace Viora.Application.Subscriptions.SubscriptionExpired;

public sealed class SubscriptionExpiredDomainEventHandler(
    ISubscriptionRepository subscriptionRepository,
    IDateTimeProvider dateTimeProvider,
    IPlanRepository planRepository,
    IFeatureUsageRepository featureUsageRepository,
    IDomainEventScheduler scheduler) : INotificationHandler<SubscriptionExpiredDomainEvent>
{
    public async Task Handle(SubscriptionExpiredDomainEvent notification, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(notification.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription {notification.SubscriptionId} was not found");

        if (subscription.SubscriptionsEndTime > dateTimeProvider.UtcNow)
        {
            await scheduler.ScheduleAsync(new SubscriptionExpiredDomainEvent(notification.SubscriptionId), subscription.SubscriptionsEndTime, cancellationToken);
        }

        var plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken)
            ?? throw new NotFoundException($"Plan {subscription.PlanId} was not found");

        var featureUsages = await featureUsageRepository.GetByOrganizationIdAsync(subscription.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Feature usage for organization {subscription.OrganizationId} was not found");

        foreach (var featureUsage in featureUsages)
            featureUsage.Expire(dateTimeProvider.UtcNow);
        subscription.Expire(dateTimeProvider.UtcNow);

    }
}
