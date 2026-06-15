using Viora.Domain.Abstractions;

namespace Viora.Domain.Appointments.Events;

public sealed record AppointmentCanceledEvent(Guid BranchId, Guid Id, DateTime ReservationDate) : IDomainEvent;
