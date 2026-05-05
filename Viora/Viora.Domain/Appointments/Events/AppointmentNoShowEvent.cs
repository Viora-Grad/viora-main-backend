using Viora.Domain.Abstractions;

namespace Viora.Domain.Appointments.Events;

public sealed record AppointmentNoShowEvent(Guid AppointmentId, DateTime ReservationDate) : IDomainEvent;
