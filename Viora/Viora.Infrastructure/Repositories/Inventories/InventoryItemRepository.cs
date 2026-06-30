using Microsoft.EntityFrameworkCore;
using Viora.Domain.Inventory;

namespace Viora.Infrastructure.Repositories.Inventories;

internal sealed class InventoryItemRepository(ApplicationDbContext dbContext)
    : Repository<InventoryItem>(dbContext), IInventoryItemRepository
{
    public async Task<IReadOnlyCollection<InventoryItem>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<InventoryItem>()
            .Where(item => item.BranchId == branchId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesDictAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return new Dictionary<Guid, string>();

        return await DbContext.Set<InventoryItem>()
            .Where(item => idList.Contains(item.Id))
            .Select(item => new { item.Id, Name = item.Name.Value })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }
}
