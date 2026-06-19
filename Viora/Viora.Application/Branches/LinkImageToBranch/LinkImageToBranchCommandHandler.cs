using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Medias;

namespace Viora.Application.Branches.LinkImageToBranch;

internal class LinkImageToBranchCommandHandler(
    IBranchRepository branchRepository,
    IMediaRepository mediaRepository,
    IBranchSettings branchSettings,
    IUnitOfWork unitOfWork) : ICommandHandler<LinkImageToBranchCommand>
{
    public async Task<Result> Handle(LinkImageToBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.GetByIdAsync(request.BranchId, cancellationToken) ?? throw new NotFoundException($"Branch {request.BranchId} was not found");
        var media = await mediaRepository.GetByIdAsync(request.MediaId, cancellationToken) ?? throw new NotFoundException($"Media {request.MediaId} was not found");

        var result = branch.AddToGallery(media, branchSettings.MaximumGallerySize);

        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

