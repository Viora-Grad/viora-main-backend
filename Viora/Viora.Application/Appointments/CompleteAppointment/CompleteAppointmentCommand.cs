using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Appointments.CompleteAppointment;

public sealed record CompleteAppointmentCommand(
    Guid AppointmentId) : ICommand;
