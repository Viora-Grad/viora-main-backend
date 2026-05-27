using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Medias.Internals;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Organizations.UpdateLogo;

internal class UpdateLogoCommandHandler(
    IOrganizationRepository organizationRepository,
    IMediaRepository mediaRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateLogoCommand>
{
    public async Task<Result> Handle(UpdateLogoCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with Id {request.OrganizationId} was not found.");

        var media = await mediaRepository.GetByIdAsync(request.MediaId, cancellationToken)
            ?? throw new NotFoundException($"Media with Id {request.MediaId} was not found.");

        if (media.CategoryType != MediaType.Image)
            return Result.Failure(OrganizationErrors.LogoMustBeAnImage);

        var result = organization.UpdateLogo(request.MediaId);
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
