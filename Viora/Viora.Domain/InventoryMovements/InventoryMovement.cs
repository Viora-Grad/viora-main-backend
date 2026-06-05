using Viora.Domain.Abstractions;
using Viora.Domain.InventoryMovements.Internals;

namespace Viora.Domain.InventoryMovements;

public sealed class InventoryMovement : Entity
{
    public Guid InventoryItemId { get; private set; }
    public Guid PerformedByUserId { get; private set; }
    public InventoryMovementType MovementType { get; private set; }
    public int Quantity { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private InventoryMovement() { }

    public static Result<InventoryMovement> Restock(Guid inventoryItemId, Guid performedByUserId, int quantity, DateTime utcNow)
    {
        if (quantity < 0)
            return Result.Failure<InventoryMovement>(InventoryMovementsErrors.QuantityInNegative);

        return Result.Success(new InventoryMovement
        {
            Id = Guid.NewGuid(),
            InventoryItemId = inventoryItemId,
            PerformedByUserId = performedByUserId,
            MovementType = InventoryMovementType.Restock,
            Quantity = quantity,
            OccurredAtUtc = utcNow
        });
    }

    public static Result<InventoryMovement> Consume(Guid inventoryItemId, Guid performedByUserId, int quantity, DateTime utcNow)
    {
        if (quantity < 0)
            return Result.Failure<InventoryMovement>(InventoryMovementsErrors.QuantityInNegative);

        return Result.Success(new InventoryMovement
        {
            Id = Guid.NewGuid(),
            InventoryItemId = inventoryItemId,
            PerformedByUserId = performedByUserId,
            MovementType = InventoryMovementType.Consume,
            Quantity = quantity,
            OccurredAtUtc = utcNow
        });
    }
}