using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Features.Internal;

namespace Viora.Domain.Plans.Features;

public class LimitedFeature : Entity
{
    public LimitKey Key { get; private set; }
    public LimitDescription Description { get; private set; }
    public int Limit { get; private set; }

    private LimitedFeature(Guid Id, LimitKey key, LimitDescription description, int limit) : base(Id)
    {
        this.Key = key;
        this.Description = description;
        this.Limit = limit;
    }
}
