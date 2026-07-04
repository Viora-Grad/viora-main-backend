using Viora.Domain.Abstractions;

namespace Viora.Domain.Reminders;

public static class ReminderErrors
{
    public static readonly Error ReminderCustomerMissing = new(
        "Reminder.CustomerMissing",
        "The reminder's customer is missing.",
        ErrorCategory.Validation
    );
    public static readonly Error ReminderAppointmentNotCompleted = new(
        "Reminder.AppointmentNotCompleted",
        "The reminder's appointment is not completed.",
        ErrorCategory.Validation
    );
}
