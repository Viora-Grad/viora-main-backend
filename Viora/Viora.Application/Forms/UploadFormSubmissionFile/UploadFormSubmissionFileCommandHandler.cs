using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Forms;
using Viora.Domain.Medias;
using Viora.Domain.Services;

namespace Viora.Application.Forms.UploadFormSubmissionFile;

internal class UploadFormSubmissionFileCommandHandler(
    IFormSubmissionRepository formSubmissionRepository,
    IMediaRepository mediaRepository,
    IDateTimeProvider dateTimeProvider,
    IStorageSettings storageSettings,
    IStorageService storageService,
    IFormRepository formRepository,
    IServiceRepository serviceRepository,
    IBranchRepository branchRepository,
    IFormSubmissionMediaRepository formSubmissionMediaRepository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<UploadFormSubmissionFileCommand>
{
    public async Task<Result> Handle(UploadFormSubmissionFileCommand request, CancellationToken cancellationToken)
    {
        var formSubmission = await formSubmissionRepository.GetByIdAsync(request.FormSubmissionId, cancellationToken)
             ?? throw new NotFoundException($"Form submission with ID {request.FormSubmissionId} not found.");

        var form = await formRepository.GetByIdAsync(formSubmission.FormId, cancellationToken)
            ?? throw new NotFoundException($"Form with ID {formSubmission.FormId} not found.");

        var service = await serviceRepository.GetByIdAsync(form.ServiceId, cancellationToken)
           ?? throw new NotFoundException($"the service with id {form.ServiceId} not found");
        var branch = await branchRepository.GetByIdAsync(service.BranchId, cancellationToken)
            ?? throw new NotFoundException($"the branch with id {service.Id} not found");

        var extension = Path.GetExtension(request.File.FileName);
        var storageKey = $"prescription-Template/{branch.OrganizationId}/{Guid.NewGuid()}{extension}";
        var mediaResult = MediaFile.Create(
            request.File.FileName,
            request.File.SizeBytes,
            storageKey,
            request.File.ContentType,
            dateTimeProvider.UtcNow,
            storageSettings.MaxFileSizeBytes,
            branch.OrganizationId
            );

        if (mediaResult.IsFailure)
            return Result.Failure<Guid>(mediaResult.Error);
        var media = mediaResult.Value;

        mediaRepository.Add(media);
        await storageService.SaveFileAsync(request.File.Content, storageKey, cancellationToken);

        var formSubmissionFile = FormSubmissionMedia.Create(
            formSubmission.Id,
            media.Id
        );

        if (formSubmissionFile.IsFailure)
            return Result.Failure(formSubmissionFile.Error);

        formSubmissionMediaRepository.Add(formSubmissionFile.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
