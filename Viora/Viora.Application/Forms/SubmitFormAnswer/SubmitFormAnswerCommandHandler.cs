using System.Text.Json;
using System.Text.Json.Nodes;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Branches;
using Viora.Domain.Forms;
using Viora.Domain.Medias;
using Viora.Domain.Services;

namespace Viora.Application.Forms.SubmitFormAnswer;

internal class SubmitFormAnswerCommandHandler(
    IAppointmentsRepository appointmentRepository,
    IUnitOfWork unitOfWork,
    IFormRepository formRepository,
    IFormSubmissionRepository formSubmissionRepository,
    IStorageService storageService,
    IStorageSettings storageSettings,
    IServiceRepository serviceRepository,
    IBranchRepository branchRepository,
    IMediaRepository mediaRepository,
    IDateTimeProvider dateTimeProvider
    ) : ICommandHandler<SubmitFormAnswerCommand>
{
    public async Task<Result> Handle(SubmitFormAnswerCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException($"the appointment with id {request.AppointmentId} not found ");

        var form = await formRepository.GetByIdAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException($"the form with id {request.FormId} not found ");
        var service = await serviceRepository.GetByIdAsync(form.ServiceId, cancellationToken)
            ?? throw new NotFoundException($"the service with id {form.ServiceId} not found");
        var branch = await branchRepository.GetByIdAsync(service.BranchId, cancellationToken)
            ?? throw new NotFoundException($"the branch with id {service.Id} not found");

        var alreadySumbit = await formSubmissionRepository.GetByAppointmentIdAsync(request.AppointmentId, request.FormId, cancellationToken);

        if (alreadySumbit != null)
            return Result.Failure(FormSubmissionError.AlreadySubmit);

        var submissionNode = JsonNode.Parse(request.Submission.RootElement.GetRawText()) as JsonObject;

        if (submissionNode is null)
            return Result.Failure(FormSubmissionError.InvalidSubmission);

        var answers = submissionNode["questions"] as JsonArray;

        if (answers is null || answers.Count == 0)
            return Result.Failure<Guid>(FormSubmissionError.QuestionsAreRequired);
        int mediaIndex = 0;

        foreach (var node in answers)
        {
            if (node is not JsonObject answer)
                continue;

            var type = answer["type"]?.GetValue<string>();

            if (!string.Equals(type, "media", StringComparison.OrdinalIgnoreCase))
                continue;

            var mediaRequest = request.MediaRequests[mediaIndex];
            var extension = Path.GetExtension(mediaRequest.FileName);
            var storageKey = $"form-submission/{request.FormId}/{request.AppointmentId}/{Guid.NewGuid()}{extension}";
            var mediaResult = MediaFile.Create(
                mediaRequest.FileName,
                mediaRequest.SizeBytes,
                storageKey,
                mediaRequest.ContentType,
                dateTimeProvider.UtcNow,
                storageSettings.MaxFileSizeBytes,
                branch.OrganizationId
            );

            if (mediaResult.IsFailure)
                return Result.Failure<Guid>(mediaResult.Error);

            mediaRepository.Add(mediaResult.Value);
            answer["answer"] = mediaResult.Value.Id.ToString();
            await storageService.SaveFileAsync(mediaRequest.Content, storageKey, cancellationToken);
            mediaIndex++;
        }

        var finalSubmission = JsonDocument.Parse(submissionNode.ToJsonString());

        var entity = FormSubmission.Create(
            request.AppointmentId,
            form.Id,
            finalSubmission,
            dateTimeProvider.UtcNow
            );

        if (entity.IsFailure)
            return Result.Failure(entity.Error);

        formSubmissionRepository.Add(entity.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();

    }
}
