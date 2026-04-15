namespace Viora.Domain.Plans.Features;

public interface IFeatureUsageRepository
{
    Task<FeatureUsage> GetByOrganizationIdAndFeatureIdAsync(Guid organizationId, Guid featureId, CancellationToken cancellationToken);
    public void Add(FeatureUsage featureUsage, CancellationToken cancellationToken);


}
