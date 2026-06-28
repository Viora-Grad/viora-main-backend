using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Appointments.DelayAppointment;

public sealed record DelayAppointmentCommand(Guid AppointmentId, TimeSpan DelayDuration) : ICommand;
