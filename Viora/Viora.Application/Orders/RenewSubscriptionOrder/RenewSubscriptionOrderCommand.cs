using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Orders.RenewSubscriptionOrder;

public sealed record RenewSubscriptionOrderCommand(
    Guid SubscriptionId) : ICommand;
