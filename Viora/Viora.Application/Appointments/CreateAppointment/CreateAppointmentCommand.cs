using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Appointments.Internal;

namespace Viora.Application.Appointments.CreateAppointment;

public sealed record CreateAppointmentCommand(
    Guid CustomerId,
    Guid ServiceId,
    Guid StaffId,
    DateTime ReservationDate,
    CustomerStatus? Status,
    Creator CreatedBy,
    Platform RequestPlatform,
    TimeSpan EstimatedDuration) : ICommand<Guid>;