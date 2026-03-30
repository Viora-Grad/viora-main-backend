using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Internal;

namespace Viora.Domain.Plans;

public class PlanLimit : Entity
{
    public Guid PlanId { get; private set; }
    public Guid LimitId { get; private set; }
    public PlanLimitValue Value { get; private set; }

    private PlanLimit(Guid Id, Guid planId, Guid limitId, PlanLimitValue value) : base(Id)
    {
        this.PlanId = planId;
        this.LimitId = limitId;
        this.Value = value;
    }
}
