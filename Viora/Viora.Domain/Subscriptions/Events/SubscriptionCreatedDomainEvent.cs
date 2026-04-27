using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions.Events;

public sealed record SubscriptionCreatedDomainEvent(
    Guid PlanId,
    Guid OrganizationId
    ) : IDomainEvent;
