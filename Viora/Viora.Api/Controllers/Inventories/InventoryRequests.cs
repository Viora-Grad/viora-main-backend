namespace Viora.Api.Controllers.Inventories;

public sealed record AddInventoryItemRequest(
    Guid BranchId,
    Guid? PhotoId,
    string Name,
    string Notes,
    int Quantity,
    int MinimumThreshold);

public sealed record UpdateInventoryItemRequest(
    Guid? PhotoId,
    string Name,
    string Notes,
    int MinimumThreshold);

public sealed record InventoryItemActionRequest(
    int Quantity,
    string? Notes);
