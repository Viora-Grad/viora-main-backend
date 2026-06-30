using Microsoft.EntityFrameworkCore;
using Viora.Domain.Inventory;
using Viora.Domain.InventoryMovements;

namespace Viora.Infrastructure.Repositories.Inventories;

internal sealed class InventoryMovementRepository(ApplicationDbContext dbContext)
    : Repository<InventoryMovement>(dbContext), IInventoryMovementRepository
{
    public async Task<IReadOnlyCollection<InventoryMovement>> GetByItemIdAsync(Guid inventiryItemId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<InventoryMovement>()
            .Where(movement => movement.InventoryItemId == inventiryItemId)
            .OrderByDescending(movement => movement.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<InventoryMovement> Items, long TotalCount)> GetByBranchAsync(
        Guid branchId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Movements have no branch id; restrict to items that belong to the branch.
        var branchItemIds = DbContext.Set<InventoryItem>()
            .Where(item => item.BranchId == branchId)
            .Select(item => item.Id);

        var query = DbContext.Set<InventoryMovement>()
            .Where(movement => branchItemIds.Contains(movement.InventoryItemId));

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(movement => movement.OccurredAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
