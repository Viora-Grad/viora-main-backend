using Viora.Application.Abstractions.Messaging;
using Viora.Application.Prescriptions.Shared;

namespace Viora.Application.Prescriptions.GetOrganizationPrescroptionTemplate;

public record GetOrganizaionPrescriptionTamplateQuery(Guid OrganizationId) : IQuery<List<TemplateResponse>>;
