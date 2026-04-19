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
    public static Result<List<FeatureUsage>> CreateMany(Guid organizationId, IEnumerable<LimitedFeature> limitedFeatures, DateTime periodStart, DateTime periodEnd)
    {
        if (limitedFeatures == null || !limitedFeatures.Any())
        {
            return Result.Failure<List<FeatureUsage>>(PlanError.InvalidPlanFeature);
        }
        var featureUsages = limitedFeatures.
            Select(feature => new FeatureUsage(Guid.NewGuid(), organizationId, feature.Id, feature.Limit, periodStart, periodEnd)).ToList();
        return Result.Success(featureUsages);
    }

    public void Renew(int value, DateTime periodEnd, DateTime periodStart)
    {
        Quota = value;
        PeriodEnd = periodEnd;
        PeriodStart = periodStart;
    }
    public void RechargeQuota(int newQuota)
    {
        Quota = newQuota;
    }
}
