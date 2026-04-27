using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions.Addons;

public class AddonOrderLimitedFeature : Entity
{
    public Guid AddonOrderId { get; private set; }
    public Guid LimitedFeatureId { get; private set; }


    private AddonOrderLimitedFeature(Guid Id, Guid addonOrderId, Guid limitedFeatureId) : base(Id)
    {
        this.AddonOrderId = addonOrderId;
        this.LimitedFeatureId = limitedFeatureId;
    }
}
