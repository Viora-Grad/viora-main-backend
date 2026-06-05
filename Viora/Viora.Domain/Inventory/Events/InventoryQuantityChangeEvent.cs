using Viora.Domain.Abstractions;

namespace Viora.Domain.Inventory.Events;

public sealed record InventoryQuantityChangeEvent(Guid InventoryItemId, Guid UserId, int Delta) : IDomainEvent;