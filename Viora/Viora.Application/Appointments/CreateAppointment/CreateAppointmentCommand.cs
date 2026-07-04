using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Appointments.CreateAppointment;

public sealed record CreateAppointmentCommand(
    Guid ServiceId,
    Guid StaffId,
    Guid? PaymentId,
    DateTime ReservationDate,
    string PaymentMethod,
    string? Status,
    string CreatedBy,
    string RequestPlatform) : ICommand<Guid>;