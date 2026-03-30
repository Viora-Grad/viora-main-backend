using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans;

public class PLanFeature : Entity
{
    public Guid PlanId { get; private set; }
    public Guid FeatureId { get; private set; }
    public bool IsEnabled { get; private set; }

    private PLanFeature(Guid id, Guid planId, Guid featureId) : base(id)
    {
        this.PlanId = planId;
        this.FeatureId = featureId;
    }
}
