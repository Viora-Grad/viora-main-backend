using Viora.Domain.Notifications;
using Viora.Domain.Notifications.Internal;

namespace Viora.Test.Compenents.Domain.Notifications;

[TestClass]
public sealed class NotificationTests
{
    // ===== Create =====

    [TestMethod]
    public void Create_ValidInput_SetsAllFieldsAndIsReadFalse()
    {
        Guid recipientId = Guid.NewGuid();
        Title title = "Appointment Reminder";
        Body body = "You have an appointment tomorrow at 10:00 AM.";
        DateTime sentAt = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

        Notification notification = Notification.Create(recipientId, title, body, sentAt);

        Assert.IsNotNull(notification);
        Assert.AreNotEqual(Guid.Empty, notification.Id);
        Assert.AreEqual(recipientId, notification.RecipientId);
        Assert.AreEqual(title, notification.Title);
        Assert.AreEqual(body, notification.Body);
        Assert.AreEqual(sentAt, notification.SentAt);
        Assert.IsFalse(notification.IsRead);
    }

    [TestMethod]
    public void Create_DifferentRecipients_CreateDistinctNotifications()
    {
        Notification n1 = Notification.Create(Guid.NewGuid(), "Title1", "Body1", DateTime.UtcNow);
        Notification n2 = Notification.Create(Guid.NewGuid(), "Title2", "Body2", DateTime.UtcNow);

        Assert.AreNotEqual(n1.Id, n2.Id);
        Assert.AreNotEqual(n1.RecipientId, n2.RecipientId);
    }

    // ===== MarkAsRead =====

    [TestMethod]
    public void MarkAsRead_UnreadNotification_SetsIsReadTrue()
    {
        Notification notification = Notification.Create(
            Guid.NewGuid(), "Title", "Body", DateTime.UtcNow);

        notification.MarkAsRead();

        Assert.IsTrue(notification.IsRead);
    }

    [TestMethod]
    public void MarkAsRead_AlreadyRead_StaysRead()
    {
        Notification notification = Notification.Create(
            Guid.NewGuid(), "Title", "Body", DateTime.UtcNow);
        notification.MarkAsRead();

        notification.MarkAsRead();

        Assert.IsTrue(notification.IsRead);
    }
}
