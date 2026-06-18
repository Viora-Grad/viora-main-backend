using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans.Features;

public class FeatureUsage : Entity
{
    public Guid OrganizationId { get; private set; }
    public Guid LimitedFeatureId { get; private set; }
    public long Quota { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public byte[] RowVersion { get; private set; } = default!;

    private FeatureUsage(Guid Id, Guid organizationId, Guid limitedFeatureId, long quota, DateTime periodStart, DateTime periodEnd) : base(Id)
    {
        this.OrganizationId = organizationId;
        this.LimitedFeatureId = limitedFeatureId;
        this.Quota = quota;
        this.PeriodStart = periodStart;
        this.PeriodEnd = periodEnd;
    }

    public void Consume(long delta)
    {
        Quota += delta;
    }

    public static Result<FeatureUsage> Create(Guid organizationId, Guid limitedFeatureId, DateTime periodStart, DateTime periodEnd, long quota)
    {
        if (limitedFeatureId == Guid.Empty)
        {
            return Result.Failure<FeatureUsage>(PlanError.InvalidPlanFeature);
        }
        var featureUsage = new FeatureUsage(Guid.NewGuid(), organizationId, limitedFeatureId, quota, periodStart, periodEnd);
        return Result.Success(featureUsage);
    }
    public static Result<List<FeatureUsage>> CreateMany(Guid organizationId, List<Guid> limitedFeaturesId, DateTime periodStart, DateTime periodEnd, long quota)
    {
        if (limitedFeaturesId == null || !limitedFeaturesId.Any())
        {
            return Result.Failure<List<FeatureUsage>>(PlanError.InvalidPlanFeature);
        }
        var featureUsages = limitedFeaturesId
            .Select(featureId => new FeatureUsage(Guid.NewGuid(), organizationId, featureId, quota, periodStart, periodEnd)).ToList();
        return Result.Success(featureUsages);
    }

    public void Renew(long value, DateTime periodEnd, DateTime periodStart)
    {
        Quota = value;
        PeriodEnd = periodEnd;
        PeriodStart = periodStart;
    }
    public void RechargeQuota(long newQuota)
    {
        Quota = newQuota;
    }

    public void AddAddon(int addonValue)
    {
        Quota += addonValue;
    }

    public void Expire(DateTime now)
    {
        PeriodEnd = now;
        Quota = 0;
    }
}
