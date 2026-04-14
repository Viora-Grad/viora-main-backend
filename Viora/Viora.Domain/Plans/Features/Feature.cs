using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Features.Internal;

namespace Viora.Domain.Plans.Features;

public class Feature : Entity
{
    public FeatureKey FeatureKey { get; private set; }
    public FeatureDescription Description { get; private set; }

    private Feature(Guid Id, FeatureKey featureKey, FeatureDescription description) : base(Id)
    {
        this.FeatureKey = featureKey;
        this.Description = description;
    }
}
