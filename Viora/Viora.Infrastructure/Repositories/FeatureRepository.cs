using Viora.Domain.Plans.Features;

namespace Viora.Infrastructure.Repositories;

internal sealed class FeatureRepository : Repository<Feature>, IFeatureRepository
{
    public FeatureRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
