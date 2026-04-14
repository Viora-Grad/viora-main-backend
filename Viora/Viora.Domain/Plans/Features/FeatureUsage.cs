using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans.Features;

public class FeatureUsage : Entity
{
    public Guid OrganizationId { get; private set; }
    public Guid LimitedFeatureId { get; private set; }
    public int Quota { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }

    private FeatureUsage(Guid Id, Guid organizationId, Guid limitedFeatureId, int Value, DateTime periodStart, DateTime periodEnd) : base(Id)
    {
        this.OrganizationId = organizationId;
        this.LimitedFeatureId = limitedFeatureId;
        this.Quota = Value;
        this.PeriodStart = periodStart;
        this.PeriodEnd = periodEnd;
    }

    public void Consume()
    {
        Quota--;
    }
}
