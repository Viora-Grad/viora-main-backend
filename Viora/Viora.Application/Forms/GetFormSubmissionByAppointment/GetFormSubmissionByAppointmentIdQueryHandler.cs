using System.Text.Json.Nodes;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Forms.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;
using Viora.Domain.Medias;

namespace Viora.Application.Forms.GetFormSubmissionByAppointment;

internal class GetFormSubmissionByAppointmentIdQueryHandler(
    //IAppontmentRepository appontmentRepository,
    IFormSubmissionRepository formSubmissionRepository,
    IMediaRepository mediaRepository
    ) : IQueryHandler<GetFormSubmissionByAppointmentQuery, FormSubmissionResponse>
{
    public async Task<Result<FormSubmissionResponse>> Handle(GetFormSubmissionByAppointmentQuery request, CancellationToken cancellationToken)
    {
        /*var appointment = await appontmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException($"the appointment with id {request.AppointmentId} not Found");*/

        var formSubmission = await formSubmissionRepository.GetByAppointmentIdAsync(request.AppointmentId, request.FormId, cancellationToken);

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
