using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Internal;

namespace Viora.Domain.Plans;

public class PlanPricingRule : Entity
{
    public Guid PlanId { get; private set; }
    public Money Price { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }

    private PlanPricingRule(Guid Id, Guid planId, Money price, BillingPeriod billingPeriod) : base(Id)
    {
        this.PlanId = planId;
        this.Price = price;
        this.BillingPeriod = billingPeriod;
    }
}
