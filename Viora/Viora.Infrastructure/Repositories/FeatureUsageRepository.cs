using Microsoft.EntityFrameworkCore;
using Viora.Domain.Plans.Features;

namespace Viora.Infrastructure.Repositories;

internal sealed class FeatureUsageRepository : Repository<FeatureUsage>, IFeatureUsageRepository
{
    public FeatureUsageRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<FeatureUsage?> GetByOrganizationIdAndFeatureIdAsync(Guid organizationId, Guid featureId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<FeatureUsage>()
            .FirstOrDefaultAsync(x =>
                x.OrganizationId == organizationId &&
                x.LimitedFeatureId == featureId,
                cancellationToken);
    }

    public void RemoveRangeByLimitedId(IEnumerable<Guid> limitedFeatureIds)
    {
        DbContext.Set<FeatureUsage>()
            .Where(fu => limitedFeatureIds.Contains(fu.LimitedFeatureId))
            .ExecuteDelete();
    }
}
