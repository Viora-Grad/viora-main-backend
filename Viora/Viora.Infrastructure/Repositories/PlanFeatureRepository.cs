using Microsoft.EntityFrameworkCore;
using Viora.Domain.Plans;

namespace Viora.Infrastructure.Repositories;

internal sealed class PlanFeatureRepository : Repository<PlanFeature>, IPlanFeatureRepository
{
    public PlanFeatureRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<PlanFeature>> GetByPlanIdAsync(Guid planId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<PlanFeature>()
            .Where(pf => pf.PlanId == planId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PlanFeature>> GetByPlanIdsAsync(List<Guid> planIds, CancellationToken cancellationToken)
    {
        if (planIds is null || !planIds.Any())
            return new List<PlanFeature>();

        var distinctIds = planIds.Distinct().ToList();

        return await DbContext.Set<PlanFeature>()
            .Where(pf => distinctIds.Contains(pf.PlanId))
            .ToListAsync(cancellationToken);
    }
}