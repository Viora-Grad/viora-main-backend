using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans.Features;

public class FeatureUsage : Entity
{
    public Guid OrganizationId { get; private set; }
    public Guid LimitedFeatureId { get; private set; }
    public int Quota { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }

    private FeatureUsage(Guid Id, Guid organizationId, Guid limitedFeatureId, int quota, DateTime periodStart, DateTime periodEnd) : base(Id)
    {
        this.OrganizationId = organizationId;
        this.LimitedFeatureId = limitedFeatureId;
        this.Quota = quota;
        this.PeriodStart = periodStart;
        this.PeriodEnd = periodEnd;
    }

    public void Consume()
    {
        Quota--;
    }
    public static Result<FeatureUsage> Create(Guid organizationId, Guid limitedFeatureId, int value, DateTime periodStart, DateTime periodEnd)
    {
        if (value < 0)
        {
            return Result.Failure<FeatureUsage>(PlanError.InvalidFeatureUsageQuota);
        }
        var featureUsage = new FeatureUsage(Guid.NewGuid(), organizationId, limitedFeatureId, value, periodStart, periodEnd);
        return Result.Success(featureUsage);
    }

    public void RechargeQuota(int newQuota)
    {
        Quota = newQuota;
    }
}
