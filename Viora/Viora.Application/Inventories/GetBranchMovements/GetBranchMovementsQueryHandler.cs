using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;
using Viora.Domain.Inventory;
using Viora.Domain.InventoryMovements;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Inventories.GetBranchMovements;

internal sealed class GetBranchMovementsQueryHandler(
    IInventoryMovementRepository movementRepository,
    IInventoryItemRepository inventoryItemRepository,
    IUserRepository userRepository) : IQueryHandler<GetBranchMovementsQuery, PaginatedModel<InventoryMovementResponse>>
{
    public async Task<Result<PaginatedModel<InventoryMovementResponse>>> Handle(GetBranchMovementsQuery request, CancellationToken cancellationToken)
    {
        var (movements, totalCount) = await movementRepository.GetByBranchAsync(
            request.BranchId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var itemNames = await inventoryItemRepository.GetNamesDictAsync(
            movements.Select(m => m.InventoryItemId),
            cancellationToken);

        var userNames = await userRepository.GetNamesDictAsync(
            movements.Select(m => m.PerformedByUserId),
            cancellationToken);

        var responses = movements.Select(m => new InventoryMovementResponse(
            m.Id,
            m.InventoryItemId,
            itemNames.TryGetValue(m.InventoryItemId, out var itemName) ? itemName : string.Empty,
            m.MovementType.ToString(),
            m.Quantity,
            m.PerformedByUserId,
            userNames.TryGetValue(m.PerformedByUserId, out var userName) ? userName : string.Empty,
            m.OccurredAtUtc)).ToList();

        return Result.Success(new PaginatedModel<InventoryMovementResponse>(responses, request.Page, request.PageSize, totalCount));
    }
}
