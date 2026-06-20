using Viora.Domain.Plans.Features;

namespace Viora.Infrastructure.Repositories.Plans;

internal sealed class FeatureRepository : Repository<Feature>, IFeatureRepository
{
    public FeatureRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
