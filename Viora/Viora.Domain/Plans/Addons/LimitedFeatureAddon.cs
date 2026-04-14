using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans.Addons;

public class LimitedFeatureAddon : Entity
{
    public Guid LimitedFeatureId { get; private set; }
    public int RestoreValue { get; private set; }
    public double Price { get; private set; }

    private LimitedFeatureAddon(Guid Id, Guid LimitedFeatureId, int RestoreValue, int price) : base(Id)
    {
        this.LimitedFeatureId = LimitedFeatureId;
        this.RestoreValue = RestoreValue;
        this.Price = price;
    }
}
