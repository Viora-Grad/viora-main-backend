using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.GetCustomerAppointment;

public sealed record GetAppointmentQuery(Guid AppointmentId) : IQuery<Appointment>;
