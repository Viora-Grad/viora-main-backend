using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Orders.CreateSubscriptionOrder;

public sealed record CreateSubscriptionOrderCommand(
    Guid OrganizationId,
    Guid PlanId) : ICommand;
