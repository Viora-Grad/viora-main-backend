using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Reminders.CreateReminder;

public sealed record CreateReminderCommand(
    Guid AppointmentId,
    string Title,
    string? Body,
    DateTime ScheduledFor
    ) : ICommand<Guid>;
