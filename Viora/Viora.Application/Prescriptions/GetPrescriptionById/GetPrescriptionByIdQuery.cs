using Viora.Application.Abstractions.Messaging;
using Viora.Application.Prescriptions.Shared;

namespace Viora.Application.Prescriptions.GetPrescriptionById;

public record GetPrescriptionByIdQuery(Guid Id) : IQuery<PrescriptionResponse>;