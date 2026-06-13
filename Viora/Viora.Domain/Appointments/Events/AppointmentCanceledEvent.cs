using Viora.Domain.Abstractions;

namespace Viora.Domain.Appointments.Events;

public sealed record AppointmentCanceledEvent(Guid Id, DateTime ReservationDate) : IDomainEvent;
