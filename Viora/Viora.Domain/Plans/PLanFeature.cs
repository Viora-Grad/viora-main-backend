using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans;

public class PlanFeature : Entity
{
    public Guid PlanId { get; private set; }
    public Guid? FeatureId { get; private set; }
    public Guid? LimitedFeatureId { get; private set; }

    private PlanFeature(Guid id, Guid planId, Guid? featureId, Guid? limitedFeatureId) : base(id)
    {
        PlanId = planId;
        FeatureId = featureId;
        LimitedFeatureId = limitedFeatureId;
    }
}
