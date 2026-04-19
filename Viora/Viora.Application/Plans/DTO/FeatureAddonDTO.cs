using Viora.Domain.Plans.Addons;

namespace Viora.Application.Plans.DTO;

public class FeatureAddonDTO
{
    Guid id { get; set; }
    Guid LimitedFeatureId { get; set; }
    public int AdditionalQuota { get; set; }
    public double Price { get; set; }


    public static FeatureAddonDTO MapToDto(LimitedFeatureAddon featureAddon)
    {
        return new FeatureAddonDTO
        {
            id = featureAddon.Id,
            LimitedFeatureId = featureAddon.LimitedFeatureId,
            AdditionalQuota = featureAddon.RestoreValue,
            Price = featureAddon.Price
        };
    }
}
