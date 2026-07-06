using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Notifications.SaveDeviceToken;

public sealed record SaveDeviceTokenCommand(string DeviceToken) : ICommand<Guid>;
