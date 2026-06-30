using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Inventory;
using Viora.Domain.Medias;
using Viora.Domain.Medias.Internals;

namespace Viora.Application.Inventories.UpdateInventoryItem;

internal class UpdateInventoryItemCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    IMediaRepository mediaRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateInventoryItemCommand>
{
    public async Task<Result> Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await inventoryItemRepository.GetByIdAsync(request.ItemId, cancellationToken) ??
            throw new NotFoundException($"Item {request.ItemId} not found");

        if (request.PhotoId != null)
        {
            var media = await mediaRepository.GetByIdAsync((Guid)request.PhotoId, cancellationToken)
                ?? throw new NotFoundException($"Image {request.PhotoId} was not found");

            if (media.CategoryType != MediaType.Image)
                return Result.Failure<Guid>(InventoryItemErrors.InventoryImageNotAllowed);
        }

        item.Update(request.Name, request.Notes, request.MinimumThreshold, request.PhotoId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
