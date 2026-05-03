using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions.Events;

public sealed record SubscriptionRenewedDomainEvent(
    Guid SubscriptionId,
    Guid PlanId,
    Guid OrganizationId) : IDomainEvent;
