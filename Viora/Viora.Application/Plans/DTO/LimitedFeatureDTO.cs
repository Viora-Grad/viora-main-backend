using Viora.Domain.Plans.Features;

namespace Viora.Application.Plans.DTO;

public class LimitedFeatureDTO
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string Description { get; set; }
    public int Limit { get; set; }



    public LimitedFeatureDTO(Guid id, string key, string description, int limit)
    {
        Id = id;
        Key = key;
        Description = description;
        Limit = limit;
    }

    public static LimitedFeatureDTO MapToDTO(LimitedFeature limitedFeature)
    {
        var limitedFeatureDTO = new LimitedFeatureDTO(
            limitedFeature.Id,
            limitedFeature.Key.value,
            limitedFeature.Description.value,
            limitedFeature.Limit
        );
        return limitedFeatureDTO;
    }

}
