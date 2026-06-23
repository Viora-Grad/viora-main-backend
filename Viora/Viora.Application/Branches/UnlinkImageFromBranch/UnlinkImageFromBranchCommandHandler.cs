using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Medias;

namespace Viora.Application.Branches.UnlinkImageFromBranch;

internal class UnlinkImageFromBranchCommandHandler(
    IBranchRepository branchRepository,
    IMediaRepository mediaRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UnlinkImageFromBranchCommand>
{
    public async Task<Result> Handle(UnlinkImageFromBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.GetByIdAsync(request.BranchId, cancellationToken) ?? throw new NotFoundException($"Branch {request.BranchId} was not found");
        var media = await mediaRepository.GetByIdAsync(request.ImageId, cancellationToken) ?? throw new NotFoundException($"Media {request.ImageId} was not found");

        var result = branch.RemoveFromGallery(media.Id);

        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
