using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions.Addons;

namespace Viora.Domain.Subscriptions;

public class SubscriptionAddon : Entity
{
    public Guid SubscriptionId { get; private set; }
    public Guid LimitedFeatureAddonId { get; private set; }
    public bool IsActive { get; private set; }
    public Subscription Subscription { get; private set; }
    public LimitedFeatureAddon LimitedFeatureAddon { get; private set; }

    private SubscriptionAddon(Guid id, Guid limitedFeatureAddonId, Guid subscriptionId) : base(id)
    {
        SubscriptionId = subscriptionId;
        LimitedFeatureAddonId = limitedFeatureAddonId;
        IsActive = true;
    }


    public static List<SubscriptionAddon> CreateMany(List<Guid> ids, Guid subscriptionId)
    {
        var addons = ids.Select(a => new SubscriptionAddon(Guid.NewGuid(), a, subscriptionId)).ToList();
        return addons;
    }

    public Result SoftDelete()
    {
        IsActive = false;
        return Result.Success();
    }
}
