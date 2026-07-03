using Viora.Domain.Abstractions;

namespace Viora.Domain.Reminders.Events;

public sealed record ReminderCreatedEvent(Guid ReminderId, Guid AppointmentId) : IDomainEvent;
