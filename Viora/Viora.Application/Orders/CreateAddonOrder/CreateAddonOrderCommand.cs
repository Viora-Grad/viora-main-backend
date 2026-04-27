using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Orders.CreateAddonOrder;

public sealed record CreateAddonOrderCommand(
    Guid OrganizationId,
    Guid SubscriptionId,
    List<Guid> AddonIds) : ICommand;

