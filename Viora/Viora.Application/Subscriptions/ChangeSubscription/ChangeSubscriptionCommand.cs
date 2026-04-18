using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Subscriptions.ChangeSubscription;

public sealed record ChangeSubscriptionCommand(Guid SubscriptionId, Guid NewPlanId) : ICommand;
