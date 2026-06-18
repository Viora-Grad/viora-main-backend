using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Features;

namespace Viora.Domain.Plans;

public class PlanLimitedFeature : Entity
{
    public Guid PlanId { get; private set; }
    public Guid LimitedFeatureId { get; private set; }
    public int LimitValue { get; private set; }

    public LimitedFeature LimitedFeature { get; private set; }
    private PlanLimitedFeature(Guid id, Guid planId, Guid limitedFeatureId, int limitValue) : base(id)
    {
        PlanId = planId;
        LimitedFeatureId = limitedFeatureId;
        LimitValue = limitValue;
    }
}
