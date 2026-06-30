using Viora.Domain.Abstractions;
using Viora.Domain.Inventory;

namespace Viora.Application.Inventories.GetInventoryItems;

internal sealed class InventoryItemSearchSpecification : BaseSpecification<InventoryItem>
{
    /// <param name="forCount">
    /// When true, ordering and paging are skipped so the spec can drive an accurate total count.
    /// </param>
    public InventoryItemSearchSpecification(Guid branchId, string? search, int page, int pageSize, bool forCount = false)
    {
        AddCriteria(item => item.BranchId == branchId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            AddCriteria(item => item.Name.Value.Contains(term));
        }

        if (forCount)
            return;

        ApplyOrderBy(item => item.Name.Value);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
