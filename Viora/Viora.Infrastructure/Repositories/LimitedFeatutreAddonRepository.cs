using Microsoft.EntityFrameworkCore;
using Viora.Domain.Subscriptions.Addons;

namespace Viora.Infrastructure.Repositories;

internal sealed class LimitedFeatutreAddonRepository : Repository<LimitedFeatureAddon>, ILimitedFeatureAddonRepository
{

    public LimitedFeatutreAddonRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }


    public async Task<List<LimitedFeatureAddon>> GetByLimitedFeatureIdAsync(Guid limitedFeatureId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<LimitedFeatureAddon>()
            .Where(a => a.LimitedFeatureId == limitedFeatureId)
            .ToListAsync(cancellationToken);
    }
}
