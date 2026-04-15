using Viora.Domain.Plans.Features;

namespace Viora.Application.Plans.GetPlans.DTO;

public class LimitedFeatureDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int Limit { get; set; }



    public LimitedFeatureDTO(Guid id, string name, int limit)
    {
        Id = id;
        Name = name;
        Limit = limit;
    }

    public static LimitedFeatureDTO MapToDTO(LimitedFeature limitedFeature)
    {
        var limitedFeatureDTO = new LimitedFeatureDTO(
            limitedFeature.Id,
            limitedFeature.Key.value,
            limitedFeature.Limit
        );
        return limitedFeatureDTO;
    }

}
