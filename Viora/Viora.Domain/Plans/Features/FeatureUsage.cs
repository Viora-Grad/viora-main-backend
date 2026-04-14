using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans.Features;

public class FeatureUsage : Entity
{
    public Guid OrganizationId { get; private set; }
    public Guid LimitedFeatureId { get; private set; }
    public int UsedValue { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }

    private FeatureUsage(Guid Id, Guid organizationId, Guid limitedFeatureId, int usedValue, DateTime periodStart, DateTime periodEnd) : base(Id)
    {
        this.OrganizationId = organizationId;
        this.LimitedFeatureId = limitedFeatureId;
        this.UsedValue = usedValue;
        this.PeriodStart = periodStart;
        this.PeriodEnd = periodEnd;
    }

}
