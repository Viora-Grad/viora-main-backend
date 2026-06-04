using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.GetOrganizationSubscriptions;

public class SubscriptionAddonResponse
{
    public Guid SubscriptionAddonId { get; set; }
    public int Value { get; set; }
    public double Price { get; set; }


    public SubscriptionAddonResponse(Guid subscriptionId, int value, double price)
    {
        SubscriptionAddonId = subscriptionId;
        Value = value;
        Price = price;
    }

    public static List<SubscriptionAddonResponse> MapToDto(List<SubscriptionAddon> addons)
    {
        var dtos = addons.Select(a => new SubscriptionAddonResponse(a.Id, a.LimitedFeatureAddon.RestoreValue, a.LimitedFeatureAddon.Price)).ToList();
        return dtos;
    }
}
