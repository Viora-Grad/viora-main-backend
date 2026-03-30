using Viora.Domain.Abstractions;

namespace Viora.Domain.Addons;

public class AddonsPrincingRule : Entity
{
    public Guid AddonId { get; private set; }
    public Money MonthlyPrice { get; private set; }
    private AddonsPrincingRule(Guid Id, Guid addonId, Money monthlyPrice) : base(Id)
    {
        this.AddonId = addonId;
        this.MonthlyPrice = monthlyPrice;
    }
}
