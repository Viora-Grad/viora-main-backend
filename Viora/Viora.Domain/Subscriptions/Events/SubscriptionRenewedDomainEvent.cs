using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions.Events;

public sealed record SubscriptionRenewedDomainEvent(
    Guid PlanId,
    Guid OrganizationId,
    DateTime StartDate,
    DateTime EndDate) : IDomainEvent;
