using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Orders.ChangeSubscriptionOrder;

public sealed record ChangeSubscriptionOrderCommand(
    Guid SubscriptionId,
    Guid NewPlanId) : ICommand;
