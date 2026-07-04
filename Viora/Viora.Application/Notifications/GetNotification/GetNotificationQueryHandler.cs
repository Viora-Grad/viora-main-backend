using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Notifications;

namespace Viora.Application.Notifications.GetNotification;

internal class GetNotificationQueryHandler(
    INotificationRepository notificationRepository) : IQueryHandler<GetNotificationQuery, Domain.Notifications.Notification>
{
    public async Task<Result<Domain.Notifications.Notification>> Handle(GetNotificationQuery request, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new NotFoundException("Notification not found");

        return Result.Success(notification);
    }
}
