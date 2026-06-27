using FirebaseAdmin.Messaging;
using Viora.Application.Notification.NotificationService;
using Viora.Domain.Abstractions;

namespace Viora.Infrastructure.NotificationService;

internal class NotificationService(
    ApplicationDbContext dbContext,
    IUnitOfWork unitOfWork,
    FirebaseMessaging firebaseMessaging) : INotificationService
{
    public async Task SaveDeviceToken(Guid userId, string deviceToken)
    {
        var existingToken = dbContext.Set<UserNotificationToken>()
        .FirstOrDefault(t => t.UserId == userId);

        if (existingToken != null)
        {
            existingToken.UpdateDeviceToken(deviceToken);
            dbContext.Set<UserNotificationToken>().Update(existingToken);
        }
        else
        {
            var userNotificationToken = new UserNotificationToken(userId, deviceToken);
            dbContext.Set<UserNotificationToken>().Add(userNotificationToken);
        }
        await unitOfWork.SaveChangesAsync();
    }

    // TODO: Implement topic-based notifications for group messages or broadcasts
    public async Task SendNotificationAsync(Guid userId, string title, string body, CancellationToken cancellationToken = default)
    {
        var userToken = await dbContext.Set<UserNotificationToken>().FindAsync(userId, cancellationToken);

        if (userToken != null)
        {
            var message = new Message()
            {
                Token = userToken.DeviceToken, // Use the device token to target the specific user or a topic for group notifications
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };
            string response = await firebaseMessaging.SendAsync(message, cancellationToken);
            return;
        }
    }
}
