using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Appointments.CheckInAppointment;

public sealed record CheckInAppointmentCommand(Guid AppointmentId) : ICommand;
