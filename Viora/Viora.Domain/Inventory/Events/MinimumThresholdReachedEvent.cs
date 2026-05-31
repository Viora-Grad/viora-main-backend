using Viora.Domain.Abstractions;

namespace Viora.Domain.Inventory.Events;

public sealed record MinimumThresholdReachedEvent(Guid ItemId) : IDomainEvent;
