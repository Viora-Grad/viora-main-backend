using Viora.Domain.Notifications;

namespace Viora.Application.Notifications.NotificationService;

public interface INotificationService
{
    Task<Guid> SaveDeviceTokenAsync(Guid userId, string deviceToken, CancellationToken cancellationToken = default);
    Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
}
