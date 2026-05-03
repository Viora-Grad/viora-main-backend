using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Subscriptions.RemoveAddon;

public sealed record RemoveAddonCommand(
    Guid SubscriptionId,
    Guid SubscriptionAddonId) : ICommand;

