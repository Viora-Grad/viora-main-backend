using Viora.Domain.Abstractions;
using Viora.Domain.Inventory.Events;
using Viora.Domain.Inventory.Internals;

namespace Viora.Domain.Inventory;

public sealed class InventoryItem : Entity
{
    public Guid BranchId { get; private set; }
    public Guid? ItemImageId { get; private set; }

    public ItemName Name { get; private set; } = default!;
    public Notes? Notes { get; private set; }

    public Quantity Quantity { get; private set; } = default!;
    public MinimumThreshold MinimumThreshold { get; private set; } = default!;

    private InventoryItem() { }

    public static InventoryItem Create(Guid branchId, string name, string notes, int quantity, int minimumThreshold, Guid? itemImageId = null)
    {
        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            Name = name,
            Notes = notes,
            Quantity = quantity,
            MinimumThreshold = minimumThreshold,
            ItemImageId = itemImageId
        };
    }

    public Result Restock(int amount, Guid userId)
    {
        Quantity = Quantity.Value + amount;
        RaiseDomainEvent(new InventoryQuantityChangeEvent(Id, userId, amount));
        return Result.Success();
    }

    public Result Consume(int amount, Guid userId)
    {
        var newQuantity = Quantity.Value - amount;

        Quantity = newQuantity;

        if (Quantity.Value <= MinimumThreshold.Value)
            RaiseDomainEvent(new MinimumThresholdReachedEvent(Id));

        RaiseDomainEvent(new InventoryQuantityChangeEvent(Id, userId, -amount));
        return Result.Success();
    }
}
