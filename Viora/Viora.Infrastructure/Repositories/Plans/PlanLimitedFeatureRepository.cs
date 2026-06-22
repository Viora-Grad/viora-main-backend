using Microsoft.EntityFrameworkCore;
using Viora.Domain.Plans;

namespace Viora.Infrastructure.Repositories.Plans;

internal class PlanLimitedFeatureRepository : Repository<PlanLimitedFeature>, IPlanLimitedFeatureRepository
{
    public PlanLimitedFeatureRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<PlanLimitedFeature?> GetPlanLimitedFeatureByLimitedFeatureIdAsync(Guid planId, Guid limitedFeatureId, CancellationToken cancellationToken)
    {
        return DbContext.Set<PlanLimitedFeature>()
            .FirstOrDefaultAsync(x => x.PlanId == planId && x.LimitedFeatureId == limitedFeatureId, cancellationToken);
    }
}
