using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Inventory;
using Viora.Domain.InventoryMovements;
using Viora.Domain.InventoryMovements.Internals;

namespace Viora.Application.Inventories.InventoryItemAction;

internal class InventoryItemActionCommandHandler(
    IInventoryItemRepository itemRepository,
    IInventoryMovementRepository movementRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<InventoryItemActionCommand>
{
    public async Task<Result> Handle(InventoryItemActionCommand request, CancellationToken cancellationToken)
    {
        var item = await itemRepository.GetByIdAsync(request.ItemId, cancellationToken)
            ?? throw new NotFoundException($"item {request.ItemId} not found");

        Result<InventoryMovement> movemntResult;

        if (request.ActionType == InventoryMovementType.Consume)
            movemntResult = Consume(request, item);
        else
            movemntResult = Restock(request, item);

        if (movemntResult.IsFailure)
            return Result.Failure(movemntResult.Error);

        movementRepository.Add(movemntResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private Result<InventoryMovement> Consume(InventoryItemActionCommand request, InventoryItem item)
    {
        var consumeResult = item.Consume(request.Quantity, request.UserId);
        if (consumeResult.IsFailure)
            return Result.Failure<InventoryMovement>(consumeResult.Error);

        var movementResult = InventoryMovement.Consume(request.ItemId, request.UserId, request.Quantity, dateTimeProvider.UtcNow);
        return movementResult;
    }

    private Result<InventoryMovement> Restock(InventoryItemActionCommand request, InventoryItem item)
    {
        var restockResult = item.Restock(request.Quantity, request.UserId);
        if (restockResult.IsFailure)
            return Result.Failure<InventoryMovement>(restockResult.Error);

        var movementResult = InventoryMovement.Restock(request.ItemId, request.UserId, request.Quantity, dateTimeProvider.UtcNow);
        return movementResult;
    }
}
