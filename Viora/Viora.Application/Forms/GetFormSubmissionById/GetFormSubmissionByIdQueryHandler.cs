using System.Text.Json.Nodes;
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
    IMediaRepository mediaRepository) : IQueryHandler<GetFormSubmissionByIdQuery, FormSubmissionResponse>
{
    public async Task<Result<FormSubmissionResponse>> Handle(GetFormSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var formSubmission = await formSubmissionRepository.GetByIdAsync(request.FormSubmissionId, cancellationToken)
            ?? throw new NotFoundException($"the submission with id {request.FormSubmissionId} Not Found");

        List<MediaResponse> mediaResponses = new List<MediaResponse>();

        var submissionNode = JsonNode.Parse(formSubmission.Submission.RootElement.GetRawText()) as JsonObject;

        if (submissionNode is null)
            return Result.Failure<FormSubmissionResponse>(FormSubmissionError.InvalidSubmission);

        var answers = submissionNode["questions"] as JsonArray;

        if (answers is null || answers.Count == 0)
            return Result.Failure<FormSubmissionResponse>(FormSubmissionError.QuestionsAreRequired);

        foreach (var node in answers)
        {
            if (node is not JsonObject answer)
                continue;

            var type = answer["type"]?.GetValue<string>();

            if (!string.Equals(type, "media", StringComparison.OrdinalIgnoreCase))
                continue;
            var mediaIdValue = answer["answer"]?.GetValue<string>();

            if (!Guid.TryParse(mediaIdValue, out var mediaId))
                return Result.Failure<FormSubmissionResponse>(FormSubmissionError.InvalidMediaId);


            var media = await mediaRepository.GetByIdAsync(mediaId, cancellationToken);

            if (media is null)
                return Result.Failure<FormSubmissionResponse>(FormSubmissionError.FileMissing);

            var mediaRespose = new MediaResponse(media.Id, media.MimeType, media.Name, media.UploadedAtUtc);
            mediaResponses.Add(mediaRespose);
        }

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
