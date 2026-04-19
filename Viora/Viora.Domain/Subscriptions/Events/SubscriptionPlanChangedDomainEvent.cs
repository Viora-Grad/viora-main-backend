using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions.Events;

public sealed record SubscriptionPlanChangedDomainEvent(
    Guid OldPlanId,
    Guid NewPlanId,
    Guid OrganizationId,
    DateTime StartTime,
    DateTime EndTime) : IDomainEvent;
