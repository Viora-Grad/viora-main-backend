using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Subscriptions.RenewSubscription;

public record RenewSubscriptionCommand(Guid SubscriptionId, string Type) : ICommand;
