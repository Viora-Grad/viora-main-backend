namespace Viora.Infrastructure.NotificationService;

internal class UserNotificationToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string DeviceToken { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private UserNotificationToken() { } // For EF Core

    private UserNotificationToken(Guid id, Guid userId, string deviceToken, DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        DeviceToken = deviceToken;
        CreatedAt = createdAt;
        IsRevoked = false;
    }
    public static UserNotificationToken Create(Guid userId, string deviceToken, DateTime utcNow)
    {
        return new UserNotificationToken(Guid.NewGuid(), userId, deviceToken, utcNow);
    }

}
