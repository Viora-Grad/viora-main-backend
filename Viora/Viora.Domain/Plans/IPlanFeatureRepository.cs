namespace Viora.Domain.Plans;

public interface IPlanFeatureRepository
{
    public Task<List<PlanFeature>> GetByPlanIdAsync(Guid planId, CancellationToken cancellationToken);
}
