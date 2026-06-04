using Viora.Domain.Subscriptions.Addons;

namespace Viora.Application.Subscriptions.GetAddons;

public class FeatureAddonResponse
{
    public Guid id { get; set; }
    public Guid LimitedFeatureId { get; set; }
    public int AdditionalQuota { get; set; }
    public double Price { get; set; }


    public static FeatureAddonResponse MapToDto(LimitedFeatureAddon featureAddon)
    {
        return new FeatureAddonResponse
        {
            id = featureAddon.Id,
            LimitedFeatureId = featureAddon.LimitedFeatureId,
            AdditionalQuota = featureAddon.RestoreValue,
            Price = featureAddon.Price
        };
    }
}
