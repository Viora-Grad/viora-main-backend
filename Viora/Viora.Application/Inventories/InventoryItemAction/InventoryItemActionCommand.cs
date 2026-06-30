using Viora.Application.Abstractions.Messaging;
using Viora.Domain.InventoryMovements.Internals;

namespace Viora.Application.Inventories.InventoryItemAction;

public sealed record InventoryItemActionCommand(Guid ItemId, Guid UserId, int Quantity, string? Notes, InventoryMovementType ActionType) : ICommand;