using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions.Addons.Event;

public sealed record AddonAddedDomainEvent(
    Guid SubscriptionId,
    List<Guid> AddonIds) : IDomainEvent;
