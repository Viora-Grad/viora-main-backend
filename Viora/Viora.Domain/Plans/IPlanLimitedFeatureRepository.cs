namespace Viora.Domain.Plans;

public interface IPlanLimitedFeatureRepository
{
    public Task<PlanLimitedFeature?> GetPlanLimitedFeatureByLimitedFeatureIdAsync(Guid planId, Guid limitedFeatureId, CancellationToken cancellationToken);
}
