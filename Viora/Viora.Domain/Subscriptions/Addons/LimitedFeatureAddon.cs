using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions.Addons.Internal;

namespace Viora.Domain.Subscriptions.Addons;

public class LimitedFeatureAddon : Entity
{
    public Guid LimitedFeatureId { get; private set; }
    public int RestoreValue { get; private set; }
    public AddonType AddonType { get; private set; }
    public double Price { get; private set; }

    private LimitedFeatureAddon(Guid Id, Guid LimitedFeatureId, int RestoreValue, AddonType AddonType, double price) : base(Id)
    {
        this.LimitedFeatureId = LimitedFeatureId;
        this.RestoreValue = RestoreValue;
        this.AddonType = AddonType;
        this.Price = price;
    }


    public static LimitedFeatureAddon Create(Guid Id, Guid LimitedFeatureId, AddonType AddonType, int RestoreValue, double price)
    {
        return new LimitedFeatureAddon(Id, LimitedFeatureId, RestoreValue, AddonType, price);
    }
}
