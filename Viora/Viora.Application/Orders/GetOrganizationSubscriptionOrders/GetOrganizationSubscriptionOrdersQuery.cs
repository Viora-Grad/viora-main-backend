using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Orders.GetOrganizationSubscriptionOrders;

public sealed record GetOrganizationSubscriptionOrdersQuery(Guid OrganizationId) : IQuery<IReadOnlyList<GetOrganizationSubscriptionOrdersResponse>>;