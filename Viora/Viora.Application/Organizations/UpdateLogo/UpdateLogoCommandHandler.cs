using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Organizations.UpdateLogo;

internal class UpdateLogoCommandHandler(
    IOrganizationRepository organizationRepository,
    IMediaRepository mediaRepository,
    IStorageSettings storageSettings,
    IStorageService storageService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateLogoCommand>
{
    public async Task<Result> Handle(UpdateLogoCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with Id {request.OrganizationId} was not found.");

        if (organization.LogoId != null)
            await DeletePrviousMediaAsync((Guid)organization.LogoId, cancellationToken);

        var extension = Path.GetExtension(request.FileName);
        var storageKey = $"logos/{request.OrganizationId}/{Guid.NewGuid()}{extension}";

        await storageService.SaveFileAsync(request.FileStream, storageKey, cancellationToken);

        var mediaResult = MediaFile.Create(
            request.FileName,
            request.SizeInBytes,
            storageKey,
            request.MimeType,
            dateTimeProvider.UtcNow,
            storageSettings.MaxFileSizeBytes,
            organization.Id);

        if (mediaResult.IsFailure)
            return Result.Failure(mediaResult.Error);

        var media = mediaResult.Value;
        mediaRepository.Add(media);

        var updateResult = organization.UpdateLogo(media.Id);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task DeletePrviousMediaAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        var media = await mediaRepository.GetByIdAsync(mediaId, cancellationToken);
        if (media != null)
            storageService.DeleteFile(media.Key);
    }
}
