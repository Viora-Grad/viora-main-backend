using Viora.Domain.Abstractions;

namespace Viora.Domain.Appointments.Events;

public sealed record AppointmentCompletedEvent(Guid Id, DateTime CompleteTime, DateTime ReservationTime) : IDomainEvent;
