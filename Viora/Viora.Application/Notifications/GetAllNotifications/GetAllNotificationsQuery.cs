using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Notifications.GetAllNotifications;

public sealed record GetAllNotificationsQuery() : IQuery<IEnumerable<Domain.Notifications.Notification>>;
