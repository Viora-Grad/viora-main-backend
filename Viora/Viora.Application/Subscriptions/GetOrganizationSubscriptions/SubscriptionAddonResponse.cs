using Viora.Application.Plans.Shared;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.GetOrganizationSubscriptions;

public class SubscriptionAddonResponse
{
    public Guid SubscriptionAddonId { get; set; }
    public int Value { get; set; }
    public MoneyResponse Price { get; set; }


    public SubscriptionAddonResponse(Guid subscriptionId, int value, MoneyResponse price)
    {
        SubscriptionAddonId = subscriptionId;
        Value = value;
        Price = price;
    }

    public static List<SubscriptionAddonResponse> MapToDto(List<SubscriptionAddon> addons)
    {
        var dtos = addons.Select(a => new SubscriptionAddonResponse(
            a.Id,
            a.LimitedFeatureAddon.RestoreValue,
            MoneyResponse.MapToDTO(a.LimitedFeatureAddon.Price)
            )).ToList();
        return dtos;
    }
}
