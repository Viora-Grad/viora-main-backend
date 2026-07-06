using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Reminders;

namespace Viora.Application.Reminders.GetReminder;

public sealed record GetReminderQuery(
    Guid ReminderId
    ) : IQuery<Reminder>;
