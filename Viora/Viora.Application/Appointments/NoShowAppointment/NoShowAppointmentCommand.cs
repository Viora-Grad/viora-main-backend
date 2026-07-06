using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Appointments.NoShowAppointment;

public sealed record NoShowAppointmentCommand(Guid AppointmentId) : ICommand;
