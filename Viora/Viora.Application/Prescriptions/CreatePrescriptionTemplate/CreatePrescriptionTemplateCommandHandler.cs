using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Prescriptions;

namespace Viora.Application.Prescriptions.CreatePrescriptionTemplate;

internal sealed class CreatePrescriptionTemplateCommandHandler(
    IOrganizationRepository organizationRepository,
    IDateTimeProvider dateTimeProvider,
    IStorageSettings storageSettings,
    IStorageService storageService,
    IPrescriptionTemplateRepository prescriptionTemplateRepository,
    IUnitOfWork unitOfWork,
    IMediaRepository mediaRepository
    ) : ICommandHandler<CreatePrescriptionTemplateCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePrescriptionTemplateCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {request.OrganizationId} not found");
        MediaFile media = null;
        if (request.File is not null)
        {
            var extension = Path.GetExtension(request.File.FileName);
            var storageKey = $"prescription-Template/{request.OrganizationId}/{Guid.NewGuid()}{extension}";
            var mediaResult = MediaFile.Create(
                request.File.FileName,
                request.File.SizeBytes,
                storageKey,
                request.File.ContentType,
                dateTimeProvider.UtcNow,
                storageSettings.MaxFileSizeBytes,
                request.OrganizationId
                );

            if (mediaResult.IsFailure)
                return Result.Failure<Guid>(mediaResult.Error);
            media = mediaResult.Value;

            mediaRepository.Add(media);
            await storageService.SaveFileAsync(request.File.Content, storageKey, cancellationToken);
        }

        var tempalteResult = PrescriptionTemplate.Create(
            request.OrganizationId,
            request.Name,
            media?.Id,
            request.TopMargin,
            request.RightMargin,
            request.LiftMargin,
            request.BottomMarign
            );

        if (tempalteResult.IsFailure)
            return Result.Failure<Guid>(tempalteResult.Error);

        prescriptionTemplateRepository.Add(tempalteResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(tempalteResult.Value.Id);
    }
}
