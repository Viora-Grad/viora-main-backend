using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Reminders;

namespace Viora.Application.Reminders.GetCustomerReminders;

public sealed record GetCustomerRemindersQuery() : IQuery<IEnumerable<Reminder>>;
