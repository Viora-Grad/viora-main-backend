using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions.Events;

public record SubscriptionExpiredDomainEvent(Guid SubscriptionId) : IDomainEvent;
