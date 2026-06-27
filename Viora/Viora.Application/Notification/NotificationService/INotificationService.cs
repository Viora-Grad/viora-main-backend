namespace Viora.Application.Notification.NotificationService;

public interface INotificationService
{
    Task SaveDeviceToken(Guid userId, string deviceToken);
    Task SendNotificationAsync(Guid userId, string title, string body, CancellationToken cancellationToken = default);
}
