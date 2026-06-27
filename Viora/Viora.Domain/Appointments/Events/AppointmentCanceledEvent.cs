using Viora.Domain.Abstractions;
using Viora.Domain.Appointments.Internal;

namespace Viora.Domain.Appointments.Events;

public sealed record AppointmentCanceledEvent(Guid Id, Guid BranchId, DateTime ReservationDate, Creator MadeBy) : IDomainEvent;
