using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Notifications.NotificationService;
using Viora.Domain.Abstractions;

namespace Viora.Application.Notifications.SaveDeviceToken;

internal class SaveDeviceTokenCommandHandler(
    INotificationService notificationService,
    IUserContext userContext,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<SaveDeviceTokenCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SaveDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var notificationId = await notificationService.SaveDeviceTokenAsync(userContext.UserId, request.DeviceToken, cancellationToken);
        return Result.Success(notificationId);
    }
}
