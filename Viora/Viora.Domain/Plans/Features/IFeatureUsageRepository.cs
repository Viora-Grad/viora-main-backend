namespace Viora.Domain.Plans.Features;

public interface IFeatureUsageRepository
{
    Task<FeatureUsage?> GetByOrganizationIdAndFeatureIdAsync(Guid organizationId, Guid featureId, CancellationToken cancellationToken);
    public void AddRange(IEnumerable<FeatureUsage> featureUsages);

    public void RemoveRange(IEnumerable<Guid> featureUsageIds);

}
