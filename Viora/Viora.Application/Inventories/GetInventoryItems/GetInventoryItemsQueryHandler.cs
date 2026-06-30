using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;
using Viora.Domain.Inventory;

namespace Viora.Application.Inventories.GetInventoryItems;

internal sealed class GetInventoryItemsQueryHandler(
    IInventoryItemRepository inventoryItemRepository) : IQueryHandler<GetInventoryItemsQuery, PaginatedModel<InventoryItemResponse>>
{
    public async Task<Result<PaginatedModel<InventoryItemResponse>>> Handle(GetInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var pageSpec = new InventoryItemSearchSpecification(request.BranchId, request.Search, request.Page, request.PageSize);
        var countSpec = new InventoryItemSearchSpecification(request.BranchId, request.Search, request.Page, request.PageSize, forCount: true);

        var items = await inventoryItemRepository.ListAsync(pageSpec, cancellationToken);
        var totalCount = await inventoryItemRepository.CountAsync(countSpec, cancellationToken);

        var responses = items.Select(item => new InventoryItemResponse(
            item.Id,
            item.BranchId,
            item.ItemImageId,
            item.Name.Value,
            item.Notes?.Value,
            item.Quantity.Value,
            item.MinimumThreshold.Value,
            item.Quantity.Value <= item.MinimumThreshold.Value)).ToList();

        return Result.Success(new PaginatedModel<InventoryItemResponse>(responses, request.Page, request.PageSize, totalCount));
    }
}
