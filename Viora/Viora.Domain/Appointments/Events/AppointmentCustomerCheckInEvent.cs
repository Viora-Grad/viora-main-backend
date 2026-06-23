using Viora.Domain.Abstractions;

namespace Viora.Domain.Appointments.Events;

public sealed record AppointmentCustomerCheckInEvent(Guid AppointmentId, DateTime CheckInTime) : IDomainEvent;

