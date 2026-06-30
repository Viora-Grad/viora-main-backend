namespace Viora.Application.Inventories.GetInventoryItems;

public sealed record InventoryItemResponse(
    Guid Id,
    Guid BranchId,
    Guid? ItemImageId,
    string Name,
    string? Notes,
    int Quantity,
    int MinimumThreshold,
    bool IsBelowThreshold);
