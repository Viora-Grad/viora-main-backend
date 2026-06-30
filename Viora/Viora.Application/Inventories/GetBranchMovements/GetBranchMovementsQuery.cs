using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;

namespace Viora.Application.Inventories.GetBranchMovements;

public sealed record GetBranchMovementsQuery(
    Guid BranchId,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedModel<InventoryMovementResponse>>;
