using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;

namespace Viora.Application.Branches.GetBranchGalleryImage;

internal sealed class GetBranchGalleryImageQueryHandler(
    IBranchRepository branchRepository,
    IStorageService storageService) : IQueryHandler<GetBranchGalleryImageQuery, MediaResponseStream>
{
    public async Task<Result<MediaResponseStream>> Handle(GetBranchGalleryImageQuery request, CancellationToken cancellationToken)
    {
        var gallery = await branchRepository.GetMediaByBranchId(request.BranchId, cancellationToken);

        var image = gallery?.FirstOrDefault(media => media.Id == request.MediaId);
        if (image is null)
            return Result.Failure<MediaResponseStream>(BranchErrors.GalleryImageNotFound);

        var stream = storageService.GetFileStream(image.Key);
        return Result.Success(new MediaResponseStream(stream, image.MimeType.Value, image.Name.Value));
    }
}
