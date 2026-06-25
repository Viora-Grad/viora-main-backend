using Viora.Domain.Orders;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions.Addons;

namespace Viora.Application.Orders.GetOrganizationAddonOrders;

public sealed record GetOrganizationAddonOrdersResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? SubscriptionId,
    Guid? InvoiceId,
    string Status,
    Money Price,
    DateTime CreatedDate,
    IReadOnlyList<AddonItemResponse> Addons)
{
    public static GetOrganizationAddonOrdersResponse MapToDto(AddonOrder order) =>
        new(
            order.Id,
            order.OrganizationId,
            order.SubscriptionId,
            order.InvoiceId,
            order.Status.Value,
            order.TotalPrice,
            order.CreatedDate,
            order.Addons.Select(AddonItemResponse.MapToDto).ToList());
}

public sealed record AddonItemResponse(
    Guid Id,
    Guid LimitedFeatureId,
    string AddonType,
    int RestoreValue,
    Money Price)
{
    public static AddonItemResponse MapToDto(LimitedFeatureAddon addon) =>
        new(
            addon.Id,
            addon.LimitedFeatureId,
            addon.AddonType.Value,
            addon.RestoreValue,
            addon.Price);
}
