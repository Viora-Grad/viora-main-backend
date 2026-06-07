using Viora.Domain.Plans.Features;

namespace Viora.Application.Plans.Shared;

public class LimitedFeatureResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string Description { get; set; }
    public int Limit { get; set; }



    public LimitedFeatureResponse(Guid id, string key, string description, int limit)
    {
        Id = id;
        Key = key;
        Description = description;
        Limit = limit;
    }

    public static LimitedFeatureResponse MapToDTO(LimitedFeature limitedFeature)
    {
        var limitedFeatureDTO = new LimitedFeatureResponse(
            limitedFeature.Id,
            limitedFeature.Key.value,
            limitedFeature.Description.value,
            limitedFeature.Limit
        );
        return limitedFeatureDTO;
    }

}
