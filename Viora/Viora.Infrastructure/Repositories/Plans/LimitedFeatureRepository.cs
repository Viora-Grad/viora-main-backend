using Viora.Domain.Plans.Features;

namespace Viora.Infrastructure.Repositories.Plans;

internal sealed class LimitedFeatureRepository : Repository<LimitedFeature>, ILimitedFeatureRepository
{
    public LimitedFeatureRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
