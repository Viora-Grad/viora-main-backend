using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;

namespace Viora.Application.Branches.GetBranchGalleryImage;

internal sealed class GetBranchGalleryImageQueryHandler(
    IBranchRepository branchRepository,
    IStorageService storageService) : IQueryHandler<GetBranchGalleryImageQuery, BranchGalleryImageResponse>
{
    public async Task<Result<BranchGalleryImageResponse>> Handle(GetBranchGalleryImageQuery request, CancellationToken cancellationToken)
    {
        // Resolve the image from the branch's own gallery. This is the security crux: the
        // media id must belong to this branch, so the route cannot be used to stream an
        // unrelated media file (e.g. a legal paper) by guessing its id.
        var gallery = await branchRepository.GetMediaByBranchId(request.BranchId, cancellationToken);

        var image = gallery?.FirstOrDefault(media => media.Id == request.MediaId);
        if (image is null)
            return Result.Failure<BranchGalleryImageResponse>(BranchErrors.GalleryImageNotFound);

        var stream = storageService.GetFileStream(image.Key);
        return Result.Success(new BranchGalleryImageResponse(stream, image.MimeType.Value, image.Name.Value));
    }
}
