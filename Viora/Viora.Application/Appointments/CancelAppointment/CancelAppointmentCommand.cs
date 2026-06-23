using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Appointments.CancelAppointment;

public sealed record CancelAppointmentCommand(Guid AppointmentId) : ICommand;
