using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;

namespace Viora.Domain.Plans;

public class Plan : Entity
{
    public PlanName Name { get; private set; }
    public PlanDescription Description { get; private set; }
    public PlanContent Content { get; private set; }
    public Money Price { get; private set; }
    public PlanPeriod PlanPeriod { get; private set; }

    public IReadOnlyCollection<PlanFeature> PlanFeatures { get; private set; } = new List<PlanFeature>();
    public IReadOnlyCollection<PlanLimitedFeature> PlanLimitedFeatures { get; private set; } = new List<PlanLimitedFeature>();

    protected Plan() { }

    private Plan(
        Guid Id,
        PlanName name,
        PlanDescription description,
        PlanContent content,
        decimal amount,
        Currency currency,
        PlanPeriod planPeriod) : base(Id)
    {
        Name = name;
        Description = description;
        Content = content;
        Price = new Money(amount, currency);
        PlanPeriod = planPeriod;
    }

    public static Plan Create(
        Guid id,
        string name,
        string description,
        string content,
        decimal amount,
        Currency currency,
        PlanPeriod planPeriod)
    {
        return new Plan(id, PlanName.Create(name), PlanDescription.Create(description), PlanContent.Create(content), amount, currency, planPeriod);
    }
}
