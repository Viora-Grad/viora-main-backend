namespace Viora.Api.Controllers.Reminders;

public sealed record CreateReminderRequest(
    Guid AppointmentId,
    string Title,
    string? Body,
    DateTime ScheduledFor
    );