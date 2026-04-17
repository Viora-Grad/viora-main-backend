using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Subscriptions.RenewSubscriptions;

public sealed record RenewSubscriptionCommand(Guid SubscriptionId) : ICommand;
