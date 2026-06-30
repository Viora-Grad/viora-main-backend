using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;

namespace Viora.Application.Inventories.GetInventoryItems;

public sealed record GetInventoryItemsQuery(
    Guid BranchId,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedModel<InventoryItemResponse>>;
