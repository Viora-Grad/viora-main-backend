using Viora.Domain.Plans.Features;

namespace Viora.Infrastructure.Repositories;

internal sealed class LimitedFeatureRepository : Repository<LimitedFeature>, ILimitedFeatureRepository
{
    public LimitedFeatureRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
