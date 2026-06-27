using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Orders.GetOrganizationAddonOrders;

public sealed record GetOrganizationAddonOrdersQuery(Guid OrganizationId) : IQuery<IReadOnlyList<GetOrganizationAddonOrdersResponse>>;