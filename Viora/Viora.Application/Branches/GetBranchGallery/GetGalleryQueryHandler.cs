using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;

namespace Viora.Application.Branches.GetBranchGallery;

public sealed class GetGalleryQueryHandler(
    IBranchRepository branchRepository) : IQueryHandler<GetBranchGalleryQuery, List<MediaResponse>>
{
    public async Task<Result<List<MediaResponse>>> Handle(GetBranchGalleryQuery request, CancellationToken cancellationToken = default)
    {
        var isBranchExisting = await branchRepository.ExistsAsync(request.BranchId, cancellationToken);
        if (!isBranchExisting)
            throw new NotFoundException($"Branch with Id {request.BranchId} could not be found");

        var gallery = await branchRepository.GetMediaByBranchId(request.BranchId, cancellationToken) ??
            throw new NotFoundException("Gallery not found");

        var result = gallery.Select(item => new MediaResponse(item.Id, item.MimeType, item.Name, item.UploadedAtUtc));

        return Result.Success(result.ToList());
    }
}
