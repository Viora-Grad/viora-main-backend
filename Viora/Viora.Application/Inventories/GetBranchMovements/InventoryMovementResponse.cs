namespace Viora.Application.Inventories.GetBranchMovements;

public sealed record InventoryMovementResponse(
    Guid MovementId,
    Guid InventoryItemId,
    string ItemName,
    string MovementType,
    int Quantity,
    Guid PerformedByUserId,
    string PerformedByName,
    DateTime OccurredAtUtc);
