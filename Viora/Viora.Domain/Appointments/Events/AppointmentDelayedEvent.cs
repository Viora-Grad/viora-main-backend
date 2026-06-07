using Viora.Domain.Abstractions;

namespace Viora.Domain.Appointments.Events;

public sealed record AppointmentDelayedEvent(
    Guid AppointmentId,
    DateTime OriginalReservationDate,
    DateTime ReservationDate,
    TimeSpan DelayDuration) : IDomainEvent
{
}
