using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Prescriptions.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Prescriptions;

namespace Viora.Application.Prescriptions.GetTemplateById;

internal class GetPrescriptionTemplateByIdQueryHandler(
    IPrescriptionTemplateRepository prescriptionTemplateRepository
    ) : IQueryHandler<GetPrescriptionTemplateByIdQuery, TemplateResponse>
{
    public async Task<Result<TemplateResponse>> Handle(GetPrescriptionTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await prescriptionTemplateRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"the prescription with id {request.Id} not found");

        var templateResponse = new TemplateResponse(
            template.Id,
            template.OrganizationId,
            new MediaResponse(template.File.Id, template.File.MimeType, template.File.Name, template.File.UploadedAtUtc),
            template.Name.Value,
            template.TopMargin,
            template.RightMargin,
            template.LeftMargin,
            template.BottomMargin
            );


        return Result.Success(templateResponse);
    }
}
