using Viora.Application.Abstractions.Messaging;
using Viora.Application.Prescriptions.Shared;

namespace Viora.Application.Prescriptions.GetPrescriptionByAppointment;

public record GetPrescriptionByAppointmentQuery(Guid AppointmentId) : IQuery<PrescriptionResponse>;
