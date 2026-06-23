using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Organizations.AddToGallery;

internal sealed class AddToGalleryCommandHandler(
    IOrganizationRepository organizationRepository,
    IStorageService storageService,
    IMediaRepository mediaRepository,
    IStorageSettings storageSettings,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<AddToGalleryCommand, IReadOnlyList<MediaResponse>>
{
    public async Task<Result<IReadOnlyList<MediaResponse>>> Handle(AddToGalleryCommand request, CancellationToken cancellationToken)
    {
        var orgExists = await organizationRepository.ExistsAsync(request.OrganizationId, cancellationToken);
        if (!orgExists)
            throw new NotFoundException($"Organization {request.OrganizationId} was not found.");

        var uploadedMedia = new List<MediaFile>(request.Medias.Count);

        foreach (var media in request.Medias)
        {
            var extension = Path.GetExtension(media.FileName);
            var storageKey = $"gallery/{request.OrganizationId}/{Guid.NewGuid()}{extension}";

            var mediaResult = MediaFile.Create(
                media.FileName,
                media.SizeBytes,
                storageKey,
                media.ContentType,
                dateTimeProvider.UtcNow,
                storageSettings.MaxFileSizeBytes,
                request.OrganizationId);

            if (mediaResult.IsFailure)
            {
                foreach (var item in uploadedMedia)
                    storageService.DeleteFile(item.Key);

                return Result.Failure<IReadOnlyList<MediaResponse>>(mediaResult.Error);
            }
            uploadedMedia.Add(mediaResult.Value);

            await storageService.SaveFileAsync(media.Content, storageKey, cancellationToken);

        }

        mediaRepository.AddRange(uploadedMedia);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = uploadedMedia
            .Select(m => new MediaResponse(m.Id, m.MimeType.Value, m.Name.Value, m.UploadedAtUtc))
            .ToList();

        return Result.Success<IReadOnlyList<MediaResponse>>(response);
    }
}
