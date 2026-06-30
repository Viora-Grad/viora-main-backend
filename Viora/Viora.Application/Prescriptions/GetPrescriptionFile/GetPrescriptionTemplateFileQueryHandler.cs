using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Prescriptions;

namespace Viora.Application.Prescriptions.GetPrescriptionFile;

internal class GetPrescriptionTemplateFileQueryHandler(
    IPrescriptionTemplateRepository prescriptionTemplateRepository,
    IStorageService storageService
    ) : IQueryHandler<GetPrescriptionTemplateFileQuery, MediaResponseStream>
{
    public async Task<Result<MediaResponseStream>> Handle(GetPrescriptionTemplateFileQuery request, CancellationToken cancellationToken)
    {
        var template = await prescriptionTemplateRepository.GetByIdAsync(request.presceiptionTemplateId, cancellationToken)
            ?? throw new NotFoundException($"the template with id {request.presceiptionTemplateId} not found");

        if (template.File is null)
            return Result.Failure<MediaResponseStream>(PrescriptionError.PrescriptionTemplateNotFound);

        var stream = storageService.GetFileStream(template.File.Key);

        var mediaResult = new MediaResponseStream(stream, template.File.MimeType, template.File.Name.Value);

        return Result.Success(mediaResult);

    }
}
