using Viora.Domain.Plans;

namespace Viora.Infrastructure.Repositories;

internal sealed class PlanRepository : Repository<Plan>, IPlanRepository
{
    public PlanRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
