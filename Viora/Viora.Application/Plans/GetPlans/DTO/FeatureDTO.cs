using Viora.Domain.Plans.Features;

namespace Viora.Application.Plans.GetPlans.DTO;

public class FeatureDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;


    public FeatureDTO(Guid id, string name)
    {
        Id = id;
        Name = name;
    }


    public static FeatureDTO MapToDTO(Feature feature)
    {
        var featureDTO = new FeatureDTO(feature.Id, feature.FeatureKey.value);
        return featureDTO;
    }
}
