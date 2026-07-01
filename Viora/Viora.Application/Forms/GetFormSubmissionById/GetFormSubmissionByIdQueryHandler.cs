using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Forms.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;
using Viora.Domain.Medias;

namespace Viora.Application.Forms.GetFormSubmissionById;

internal class GetFormSubmissionByIdQueryHandler(
    IFormSubmissionRepository formSubmissionRepository,
    IFormSubmissionMediaRepository formSubmissionMediaRepository,
    IDateTimeProvider dateTimeProvider,
    IMediaRepository mediaRepository) : IQueryHandler<GetFormSubmissionByIdQuery, FormSubmissionResponse>
{
    public async Task<Result<FormSubmissionResponse>> Handle(GetFormSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var formSubmission = await formSubmissionRepository.GetByIdAsync(request.FormSubmissionId, cancellationToken)
            ?? throw new NotFoundException($"the submission with id {request.FormSubmissionId} Not Found");

        var formSubmissionFiles = await formSubmissionMediaRepository.GetByFormSubmissionIdAsync(request.FormSubmissionId, cancellationToken)
            ?? throw new NotFoundException($"No files found for form submission with ID {request.FormSubmissionId}.");

        var mediaIds = formSubmissionFiles.Select(f => f.MediaId).ToList();

        var mediaFiles = await mediaRepository.GetByIdsAsync(mediaIds, cancellationToken);

        var mediaResponses = mediaFiles.Select(mf => new MediaResponse(
            mf.Id,
            mf.MimeType,
            mf.Name,
            dateTimeProvider.UtcNow
            )
        ).ToList();

        var formSubmissionResponse = new FormSubmissionResponse(
            formSubmission.Id,
            formSubmission.AppointmentId,
            formSubmission.FormId,
            formSubmission.Submission,
            formSubmission.CreatedAt,
            mediaResponses
            );

        return Result.Success(formSubmissionResponse);

    }
}
