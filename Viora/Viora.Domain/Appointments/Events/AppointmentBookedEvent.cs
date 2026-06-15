using Viora.Domain.Abstractions;

namespace Viora.Domain.Appointments.Events;

public sealed record AppointmentBookedEvent(
    Guid BranchId,
    Guid AppointmentId,
    DateTime ReservationDate) : IDomainEvent;
