namespace Viora.Infrastructure.NotificationService;

internal class UserNotificationToken
{
    public Guid UserId { get; private set; }
    public string DeviceToken { get; private set; } = null!;

    private UserNotificationToken() { } // For EF Core

    public UserNotificationToken(Guid userId, string deviceToken)
    {
        UserId = userId;
        DeviceToken = deviceToken;
    }
    public void UpdateDeviceToken(string newDeviceToken)
    {
        DeviceToken = newDeviceToken;
    }
}
