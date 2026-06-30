namespace Viora.Application.Inventories.GetInventoryItemById;

public sealed record InventoryItemDetailsResponse(
    Guid Id,
    Guid BranchId,
    Guid? ItemImageId,
    string Name,
    string? Notes,
    int Quantity,
    int MinimumThreshold,
    bool IsBelowThreshold);
