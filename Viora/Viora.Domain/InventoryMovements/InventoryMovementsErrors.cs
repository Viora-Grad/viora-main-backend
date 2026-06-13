using Viora.Domain.Abstractions;

namespace Viora.Domain.InventoryMovements;

public static class InventoryMovementsErrors
{
    public readonly static Error QuantityInNegative
        = new("InventoryMovements.QuantityInNegative", "The Operation can not be done for negative values", ErrorCategory.Validation);
}
