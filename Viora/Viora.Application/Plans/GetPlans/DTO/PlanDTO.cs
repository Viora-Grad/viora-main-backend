using Viora.Domain.Plans;

namespace Viora.Application.Plans.GetPlans.DTO;

public class PlanDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public double Price { get; set; }
    public string PlanPeriod { get; set; } = default!;
    public List<LimitedFeatureDTO> LimitedFeatures { get; set; } = default!;
    public List<FeatureDTO> Features { get; set; } = default!;


    public PlanDTO(Guid id, string name, string description, double price, string planPeriod, List<LimitedFeatureDTO> limitedFeatures, List<FeatureDTO> features)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        PlanPeriod = planPeriod;
        LimitedFeatures = limitedFeatures;
        Features = features;
    }

    public static PlanDTO MapToDTO(Plan plan, List<FeatureDTO> feature, List<LimitedFeatureDTO> limitedFeature)
    {
        return new PlanDTO(
            plan.Id,
            plan.Name.value,
            plan.Description.Value,
            plan.Price,
            plan.PlanPeriod.ToString(),
            limitedFeature,
            feature
        );
    }

}
