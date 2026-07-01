using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;
using Viora.Domain.Medias;

namespace Viora.Application.Forms.GetFormSubmissionFile;

internal class GetFormSubmissionFileQueryHandler(
    IFormSubmissionRepository formSubmissionRepository,
    IMediaRepository mediaRepository,
    IStorageService storageService
    ) : IQueryHandler<GetFormSubmissionFileQuery, MediaResponseStream>
{
    public async Task<Result<MediaResponseStream>> Handle(GetFormSubmissionFileQuery request, CancellationToken cancellationToken)
    {
        var formSubmission = await formSubmissionRepository.GetByIdAsync(request.FormSubmissionId, cancellationToken)
            ?? throw new NotFoundException($"Form submission with ID {request.FormSubmissionId} not found.");

        var mediaFile = await mediaRepository.GetByIdAsync(request.FileId, cancellationToken)
            ?? throw new NotFoundException($"Media file with ID {request.FileId} not found.");


        var stream = storageService.GetFileStream(mediaFile.Key);

        var mediaResult = new MediaResponseStream(stream, mediaFile.MimeType, mediaFile.Name.Value);

        return Result.Success(mediaResult);

    }
}
