using Viora.Domain.Abstractions;

namespace Viora.Domain.Inventory;

public interface IInventoryItemRepository
{
    public void Add(InventoryItem item);
    public Task<IReadOnlyCollection<InventoryItem>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken);
    public Task<InventoryItem?> GetByIdAsync(Guid Id, CancellationToken cancellationToken);

    Task<IReadOnlyList<InventoryItem>> ListAsync(ISpecification<InventoryItem> spec, CancellationToken cancellationToken = default);
    Task<long> CountAsync(ISpecification<InventoryItem> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a map of item id to display name for the given item ids (missing ids are omitted).
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> GetNamesDictAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
