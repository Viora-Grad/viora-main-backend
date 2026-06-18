using Microsoft.EntityFrameworkCore;
using Viora.Domain.Plans;

namespace Viora.Infrastructure.Repositories.Plans;

internal sealed class PlanRepository : Repository<Plan>, IPlanRepository
{
    public PlanRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }


    public override async Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Plan>()
            .Include(p => p.PlanFeatures)
                .ThenInclude(pf => pf.Feature)
            .Include(p => p.PlanLimitedFeatures)
                .ThenInclude(plf => plf.LimitedFeature)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<List<Plan>> GetAllAsNoTrackingAsync(CancellationToken cancellationToken)
    {
        return await DbContext.Set<Plan>()
            .Include(p => p.PlanFeatures)
                .ThenInclude(pf => pf.Feature)
            .Include(p => p.PlanLimitedFeatures)
                .ThenInclude(plf => plf.LimitedFeature)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
