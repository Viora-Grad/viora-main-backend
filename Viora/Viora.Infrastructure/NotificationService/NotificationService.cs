using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Notifications.NotificationService;
using Viora.Domain.Abstractions;

namespace Viora.Infrastructure.NotificationService;

internal class NotificationService(
    ApplicationDbContext dbContext,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    FirebaseMessaging firebaseMessaging) : INotificationService
{
    public async Task<Guid> SaveDeviceTokenAsync(Guid userId, string deviceToken, CancellationToken cancellationToken = default)
    {
        var notification = UserNotificationToken.Create(userId, deviceToken, dateTimeProvider.UtcNow);
        dbContext.Set<UserNotificationToken>()
            .Add(notification);
        await dbContext.SaveChangesAsync(cancellationToken);
        return notification.Id;
    }

    public async Task SendNotificationAsync(Domain.Notifications.Notification notification, CancellationToken cancellationToken = default)
    {

        // Implementation for sending notification
        var tokens = await dbContext.Set<UserNotificationToken>()
            .Where(t => t.UserId == notification.RecipientId)
            .Select(t => t.DeviceToken).ToListAsync(cancellationToken);


        foreach (var token in tokens)
        {
            var message = new Message()
            {
                Token = token,
                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = notification.Title.Value,
                    Body = notification.Body.Value
                }
            };
            await firebaseMessaging.SendAsync(message, cancellationToken);
        }
    }
}
