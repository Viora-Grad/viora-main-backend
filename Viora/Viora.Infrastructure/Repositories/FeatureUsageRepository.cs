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

    public async Task<List<FeatureUsage>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<FeatureUsage>()
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
    }

    public void RemoveRangeByLimitedIdAndOrganizationId(IEnumerable<Guid> limitedFeatureIds, Guid organizationId)
    {
        DbContext.Set<FeatureUsage>()
            .Where(fu => limitedFeatureIds.Contains(fu.LimitedFeatureId) &&
            fu.OrganizationId == organizationId)
            .ExecuteDelete();
    }

}
