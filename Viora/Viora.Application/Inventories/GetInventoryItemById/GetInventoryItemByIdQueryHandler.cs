using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Inventory;

namespace Viora.Application.Inventories.GetInventoryItemById;

internal sealed class GetInventoryItemByIdQueryHandler(
    IInventoryItemRepository inventoryItemRepository) : IQueryHandler<GetInventoryItemByIdQuery, InventoryItemDetailsResponse>
{
    public async Task<Result<InventoryItemDetailsResponse>> Handle(GetInventoryItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await inventoryItemRepository.GetByIdAsync(request.ItemId, cancellationToken);

        if (item is null)
            return Result.Failure<InventoryItemDetailsResponse>(InventoryItemErrors.NotFound);

        var response = new InventoryItemDetailsResponse(
            item.Id,
            item.BranchId,
            item.ItemImageId,
            item.Name.Value,
            item.Notes?.Value,
            item.Quantity.Value,
            item.MinimumThreshold.Value,
            item.Quantity.Value <= item.MinimumThreshold.Value);

        return Result.Success(response);
    }
}
