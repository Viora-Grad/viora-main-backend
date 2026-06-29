using Viora.Domain.Abstractions;

namespace Viora.Domain.Appointments.Events;

public sealed record AppointmentCheckedInEvent(Guid Id, DateTime CheckInTime) : IDomainEvent;
