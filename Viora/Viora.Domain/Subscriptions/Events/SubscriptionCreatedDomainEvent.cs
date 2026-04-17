using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions.Events;

public sealed record SubscriptionCreatedDomainEvent(
    Guid SubscriptionId,
    Guid PlanId,
    Guid OrganizationId,
    DateTime SubscriptionsStartTime,
    DateTime SubscriptionEndTime
    ) : IDomainEvent;
