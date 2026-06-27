using Viora.Domain.Orders;
using Viora.Domain.Shared;

namespace Viora.Application.Orders.GetOrganizationSubscriptionOrders;

public sealed record GetOrganizationSubscriptionOrdersResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? SubscriptionId,
    Guid? InvoiceId,
    Guid PlanId,
    string PlanName,
    Money Price,
    DateTime CreatedAtUtc,
    string OrderType,
    string Status)
{
    public static GetOrganizationSubscriptionOrdersResponse MapToDto(SubscriptionOrder order, string planName)
    {
        return new(order.Id,
            order.OrganizationId,
            order.SubscriptionId,
            order.InvoiceId,
            order.PlanId,
            planName,
            order.TotalPrice,
            order.CreatedDate,
            order.SubscriptionOrderType.Value,
            order.Status.Value);
    }
}