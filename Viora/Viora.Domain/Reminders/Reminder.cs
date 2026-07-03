using Viora.Domain.Abstractions;
using Viora.Domain.Reminders.Internal;

namespace Viora.Domain.Reminders;

public sealed class Reminder : Entity
{
    public Guid AppointmentId { get; private set; }
    public TItle Title { get; private set; } = null!;
    public Body? Body { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }
    public DateTime ScheduledFor { get; private set; }

    private Reminder() { }

    private Reminder(Guid id, Guid appointmentId, TItle title, Body? body, DateTime createdAt, DateTime scheduledFor)
        : base(id)
    {
        AppointmentId = appointmentId;
        Title = title;
        Body = body;
        CreatedAt = createdAt;
        ScheduledFor = scheduledFor;
    }
    public static Reminder Create(Guid appointmentId, TItle title, Body? body, DateTime createdAt, DateTime scheduledFor)
    {
        var reminder = new Reminder(Guid.NewGuid(), appointmentId, title, body, createdAt, scheduledFor);

        //reminder.RaiseDomainEvent(new ReminderCreatedEvent(reminder.Id, appointmentId)); scheduler will handle the event and send the reminder to the user at the scheduled time

        return reminder;
    }
}
