using Viora.Application.Abstractions.Messaging;
namespace Viora.Application.Notifications.GetNotification;

public sealed record GetNotificationQuery(Guid Id) : IQuery<Domain.Notifications.Notification>;

