using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;

namespace Viora.Application.Plans.Shared;

public class PlanResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string PlanPeriodTime { get; set; } = default!;
    public string PlanContent { get; set; } = default!;
    public MoneyResponse Price { get; set; }
    public List<LimitedFeatureResponse> LimitedFeatures { get; set; } = default!;
    public List<FeatureResponse> Features { get; set; } = default!;


    public PlanResponse(Guid id, string name, string description, MoneyResponse price, string planPeriod, string planContent, List<LimitedFeatureResponse> limitedFeatures, List<FeatureResponse> features)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        PlanPeriodTime = planPeriod;
        PlanContent = planContent;
        LimitedFeatures = limitedFeatures;
        Features = features;
    }

    public static PlanResponse MapToDTO(Plan plan, List<FeatureResponse> feature, List<LimitedFeatureResponse> limitedFeature)
    {
        var planPeriod = PlanPeriod.FromId(plan.PlanPeriod.Id);
        return new PlanResponse(
            plan.Id,
            plan.Name.value,
            plan.Description.Value,
            MoneyResponse.MapToDTO(plan.Price),
            planPeriod.Name,
            plan.Content.Value,
            limitedFeature,
            feature
        );
    }

}
