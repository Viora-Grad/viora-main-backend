using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Inventory;
using Viora.Domain.InventoryMovements;
using Viora.Domain.Medias;
using Viora.Domain.Medias.Internals;

namespace Viora.Application.Inventories.AddToInventory;

internal class AddToInventoryCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    IInventoryMovementRepository inventoryMovementRepository,
    IMediaRepository mediaRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<AddToInventoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddToInventoryCommand request, CancellationToken cancellationToken)
    {
        if (request.PhotoId != null)
        {
            var media = await mediaRepository.GetByIdAsync((Guid)request.PhotoId, cancellationToken)
                ?? throw new NotFoundException($"Image {request.PhotoId} was not found");

            if (media.CategoryType != MediaType.Image)
                return Result.Failure<Guid>(InventoryItemErrors.InventoryImageNotAllowed);
        }

        var item = InventoryItem.Create(
            request.BranchId,
            request.Name,
            request.Notes,
            request.Quantity,
            request.MinimumThreshold,
            request.PhotoId);

        inventoryItemRepository.Add(item);

        var movementResult = InventoryMovement.Restock(item.Id, request.UserId, request.Quantity, dateTimeProvider.UtcNow);

        if (movementResult.IsFailure)
            return Result.Failure<Guid>(movementResult.Error);

        inventoryMovementRepository.Add(movementResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(item.Id);
    }
}
