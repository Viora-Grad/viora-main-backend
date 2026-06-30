using Viora.Application.Abstractions.Messaging;
using Viora.Application.Prescriptions.Shared;

namespace Viora.Application.Prescriptions.GetTemplateById;

public record GetPrescriptionTemplateByIdQuery(Guid Id) : IQuery<TemplateResponse>;
