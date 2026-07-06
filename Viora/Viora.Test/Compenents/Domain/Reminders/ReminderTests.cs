using Viora.Domain.Reminders;
using Viora.Domain.Reminders.Internal;

namespace Viora.Test.Compenents.Domain.Reminders;

[TestClass]
public sealed class ReminderTests
{
    // ===== Create =====

    [TestMethod]
    public void Create_ValidInput_SetsAllFields()
    {
        Guid appointmentId = Guid.NewGuid();
        TItle title = "Follow-up reminder";
        Body body = "Please remember your follow-up appointment.";
        DateTime createdAt = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);
        DateTime scheduledFor = new(2026, 7, 8, 9, 0, 0, DateTimeKind.Utc);

        Reminder reminder = Reminder.Create(appointmentId, title, body, createdAt, scheduledFor);

        Assert.IsNotNull(reminder);
        Assert.AreNotEqual(Guid.Empty, reminder.Id);
        Assert.AreEqual(appointmentId, reminder.AppointmentId);
        Assert.AreEqual(title, reminder.Title);
        Assert.AreEqual(body, reminder.Body);
        Assert.AreEqual(createdAt, reminder.CreatedAt);
        Assert.AreEqual(scheduledFor, reminder.ScheduledFor);
    }

    [TestMethod]
    public void Create_WithNullBody_SetsBodyToNull()
    {
        Guid appointmentId = Guid.NewGuid();
        TItle title = "Reminder";
        DateTime createdAt = DateTime.UtcNow;
        DateTime scheduledFor = DateTime.UtcNow.AddDays(1);

        Reminder reminder = Reminder.Create(appointmentId, title, null, createdAt, scheduledFor);

        Assert.IsNull(reminder.Body);
    }

    [TestMethod]
    public void Create_GeneratesNewIdEachCall()
    {
        Reminder r1 = Reminder.Create(Guid.NewGuid(), "Title1", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        Reminder r2 = Reminder.Create(Guid.NewGuid(), "Title2", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

        Assert.AreNotEqual(r1.Id, r2.Id);
    }
}
