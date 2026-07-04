using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Appointments.GetAppointment;

public sealed record GetAppointmentQuery(Guid AppointmentId) : IQuery<GetAppointmentResponse>;
