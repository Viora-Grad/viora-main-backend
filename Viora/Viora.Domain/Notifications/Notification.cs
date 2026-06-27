using Viora.Domain.Abstractions;
using Viora.Domain.Notifications.Internal;

namespace Viora.Domain.Notifications;

public sealed class Notification : Entity
{
    public Guid RecipientId { get; private set; }
    public Title Title { get; private set; } = null!;
    public Body Body { get; private set; } = null!;
    public DateTime SentAt { get; private set; }
    public bool IsRead { get; private set; } = false;
    private Notification() { } // For EF Core
    private Notification(Guid id, Guid recipientId, Title title, Body body, DateTime sentAt) : base(id)
    {
        RecipientId = recipientId;
        Title = title;
        Body = body;
        SentAt = sentAt;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
    public static Notification Create(Guid recipientId, Title title, Body body, DateTime utcNow)
    {
        return new Notification(Guid.NewGuid(), recipientId, title, body, utcNow);
    }
}
