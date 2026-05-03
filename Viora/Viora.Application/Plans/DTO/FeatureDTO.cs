using Viora.Domain.Plans.Features;

namespace Viora.Application.Plans.DTO;

public class FeatureDTO
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string Description { get; set; } = default!;


    public FeatureDTO(Guid id, string key, string description)
    {
        Id = id;
        Key = key;
        Description = description;
    }


    public static FeatureDTO MapToDTO(Feature feature)
    {
        var featureDTO = new FeatureDTO(feature.Id, feature.FeatureKey.value, feature.Description.value);
        return featureDTO;
    }
}
