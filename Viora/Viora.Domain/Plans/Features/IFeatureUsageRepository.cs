namespace Viora.Domain.Plans.Features;

public interface IFeatureUsageRepository
{
    Task<FeatureUsage?> GetByOrganizationIdAndFeatureIdAsync(Guid organizationId, Guid featureId, CancellationToken cancellationToken);
    public void AddRange(IEnumerable<FeatureUsage> featureUsages);
    public void Add(FeatureUsage featureUsage);
    Task<List<FeatureUsage>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken);
    public void RemoveRangeByLimitedIdAndOrganizationId(IEnumerable<Guid> limitedFeatureIds, Guid organizationId);

}
