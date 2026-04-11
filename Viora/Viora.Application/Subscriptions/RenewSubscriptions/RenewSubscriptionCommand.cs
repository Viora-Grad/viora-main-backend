using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Subscriptions.RenewSubscriptions;

public record RenewSubscriptionCommand(Guid SubscriptionId) : ICommand;
