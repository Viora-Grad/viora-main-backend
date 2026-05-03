using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions.Addons.Internal;

namespace Viora.Domain.Subscriptions.Addons;

public class LimitedFeatureAddon : Entity
{
    public Guid LimitedFeatureId { get; private set; }
    public int RestoreValue { get; private set; }
    public AddonType AddonType { get; private set; }
    public double Price { get; private set; }

    private LimitedFeatureAddon(Guid Id, Guid LimitedFeatureId, int RestoreValue, double price) : base(Id)
    {
        this.LimitedFeatureId = LimitedFeatureId;
        this.RestoreValue = RestoreValue;
        this.Price = price;
    }
}
