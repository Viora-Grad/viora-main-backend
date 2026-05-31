using Viora.Domain.Plans.Features;

namespace Viora.Application.Plans.Shared;

public class FeatureResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string Description { get; set; } = default!;


    public FeatureResponse(Guid id, string key, string description)
    {
        Id = id;
        Key = key;
        Description = description;
    }


    public static FeatureResponse MapToDTO(Feature feature)
    {
        var featureDTO = new FeatureResponse(feature.Id, feature.FeatureKey.value, feature.Description.value);
        return featureDTO;
    }
}
