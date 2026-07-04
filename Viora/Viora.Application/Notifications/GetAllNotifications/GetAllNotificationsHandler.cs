using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Notifications;

namespace Viora.Application.Notifications.GetAllNotifications;

internal class GetAllNotificationsHandler(
    IUserContext userContext,
    INotificationRepository notificationRepository
    ) : IQueryHandler<GetAllNotificationsQuery, IEnumerable<Domain.Notifications.Notification>>
{
    public async Task<Result<IEnumerable<Notification>>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var notifications = await notificationRepository.GetByUserIdAsync(userId, cancellationToken);
        return Result.Success(notifications.AsEnumerable());
    }
}
