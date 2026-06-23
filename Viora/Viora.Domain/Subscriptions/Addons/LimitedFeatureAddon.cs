using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions.Addons.Internal;

namespace Viora.Domain.Subscriptions.Addons;

public class LimitedFeatureAddon : Entity
{
    public Guid LimitedFeatureId { get; private set; }
    public int RestoreValue { get; private set; }
    public AddonType AddonType { get; private set; }
    public Money Price { get; private set; }


    protected LimitedFeatureAddon() { }
    private LimitedFeatureAddon(Guid Id, Guid LimitedFeatureId, int RestoreValue, AddonType AddonType, Money price) : base(Id)
    {
        this.LimitedFeatureId = LimitedFeatureId;
        this.RestoreValue = RestoreValue;
        this.AddonType = AddonType;
        this.Price = price;
    }


    public static LimitedFeatureAddon Create(Guid Id, Guid LimitedFeatureId, AddonType AddonType, int RestoreValue, Money price)
    {
        return new LimitedFeatureAddon(Id, LimitedFeatureId, RestoreValue, AddonType, price);
    }
}
