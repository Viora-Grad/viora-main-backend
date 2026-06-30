namespace Viora.Domain.InventoryMovements;

public interface IInventoryMovementRepository
{
    public void Add(InventoryMovement movement);
    public Task<IReadOnlyCollection<InventoryMovement>> GetByItemIdAsync(Guid inventiryItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a page of movements (newest first) for every item belonging to the branch,
    /// along with the total count. Movements carry no branch id, so this joins to the items.
    /// </summary>
    Task<(IReadOnlyList<InventoryMovement> Items, long TotalCount)> GetByBranchAsync(
        Guid branchId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
