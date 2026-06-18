using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Features;

namespace Viora.Domain.Plans;

public class PlanFeature : Entity
{
    public Guid PlanId { get; private set; }
    public Guid FeatureId { get; private set; }

    public IReadOnlyCollection<Feature> features { get; private set; } = new List<Feature>();

    private PlanFeature(Guid id, Guid planId, Guid featureId) : base(id)
    {
        PlanId = planId;
        FeatureId = featureId;
    }
}
