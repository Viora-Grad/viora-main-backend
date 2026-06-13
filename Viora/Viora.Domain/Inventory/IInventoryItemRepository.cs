namespace Viora.Domain.Inventory;

internal interface IInventoryItemRepository
{
    public void Add(InventoryItem item);
    public IReadOnlyCollection<InventoryItem> GetByBranch(Guid branchId);
    public void Restock(Guid itemId, int addedAmount);
    public void Consume(Guid itemId, int conumedAmount);
}
