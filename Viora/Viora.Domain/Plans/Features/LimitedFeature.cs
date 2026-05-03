using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Features.Internal;

namespace Viora.Domain.Plans.Features;

public class LimitedFeature : Entity
{
    public FeatureKey Key { get; private set; }
    public FeatureDescription Description { get; private set; }
    public int Limit { get; private set; }

    private LimitedFeature(Guid Id, FeatureKey key, FeatureDescription description, int limit) : base(Id)
    {
        this.Key = key;
        this.Description = description;
        this.Limit = limit;
    }
}
