using Viora.Domain.Abstractions;

namespace Viora.Domain.Inventory;

public static class InventoryItemErrors
{
    public static readonly Error InventoryImageNotAllowed = new("InventoryItems.InventoryImageNotAllowed", "only image/png, image/jpeg, image/webp is allowed", ErrorCategory.Validation);
    public static readonly Error NotFound = new("InventoryItems.NotFound", "The inventory item was not found", ErrorCategory.NotFound);
    public static readonly Error QuantityNegative = new("InventoryItems.QuantityNegative", "Not enough items to consume", ErrorCategory.Validation);

}
