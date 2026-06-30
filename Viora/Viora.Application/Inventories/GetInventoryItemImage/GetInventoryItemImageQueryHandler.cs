using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Inventory;
using Viora.Domain.Medias;

namespace Viora.Application.Inventories.GetInventoryItemImage;

internal sealed class GetInventoryItemImageQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    IMediaRepository mediaRepository,
    IStorageService storageService) : IQueryHandler<GetInventoryItemImageQuery, MediaResponseStream>
{
    public async Task<Result<MediaResponseStream>> Handle(GetInventoryItemImageQuery request, CancellationToken cancellationToken)
    {
        var item = await inventoryItemRepository.GetByIdAsync(request.ItemId, cancellationToken);

        if (item is null)
            return Result.Failure<MediaResponseStream>(InventoryItemErrors.NotFound);

        var imageId = item.ItemImageId
            ?? throw new NotFoundException($"Inventory item {request.ItemId} does not have an image");

        var media = await mediaRepository.GetByIdAsync(imageId, cancellationToken)
            ?? throw new NotFoundException($"Media {imageId} not found");

        var stream = storageService.GetFileStream(media.Key);
        return Result.Success(new MediaResponseStream(stream, media.MimeType.Value, media.Name.Value));
    }
}
