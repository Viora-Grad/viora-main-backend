using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Internal;

namespace Viora.Domain.Plans;

public class Plan : Entity
{
    public PlanName Name { get; private set; }
    public PlanDescription Description { get; private set; }
    public PlanContent Content { get; private set; }
    public double Price { get; private set; }
    public PlanPeriod PlanPeriod { get; private set; }


    private Plan(
        Guid Id,
        PlanName name,
        PlanDescription description,
        PlanContent content,
        double price,
        PlanPeriod planPeriod) : base(Id)
    {
        Name = name;
        Description = description;
        Content = content;
        Price = price;
        PlanPeriod = planPeriod;
    }
}
