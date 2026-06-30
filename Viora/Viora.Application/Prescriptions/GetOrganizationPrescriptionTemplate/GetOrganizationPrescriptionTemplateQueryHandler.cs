using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Prescriptions.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Prescriptions;

namespace Viora.Application.Prescriptions.GetOrganizationPrescroptionTemplate;

internal sealed class GetOrganizationPrescriptionTemplateQueryHandler(
    IOrganizationRepository organizationRepository,
    IPrescriptionTemplateRepository prescriptionTemplateRepository
    ) : IQueryHandler<GetOrganizaionPrescriptionTamplateQuery, List<TemplateResponse>>
{
    public async Task<Result<List<TemplateResponse>>> Handle(GetOrganizaionPrescriptionTamplateQuery request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {request.OrganizationId} not Found");

        var organizationPrescriptions = await prescriptionTemplateRepository.GetByOrganizationAsync(organization.Id, cancellationToken);

        if (organizationPrescriptions == null || organizationPrescriptions.Any())
            return Result.Failure<List<TemplateResponse>>(PrescriptionError.PrescriptionTemplateNotFound);

        var templateResponses = organizationPrescriptions.Select(op => new TemplateResponse(
            op.Id,
            op.OrganizationId,
            new MediaResponse(op.File.Id, op.File.MimeType, op.File.Name, op.File.UploadedAtUtc),
            op.Name.Value,
            op.TopMargin,
            op.RightMargin,
            op.LeftMargin,
            op.BottomMargin
            )
        ).ToList();

        return Result.Success(templateResponses);
    }
}
