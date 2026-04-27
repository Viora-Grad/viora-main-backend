using Microsoft.EntityFrameworkCore;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Infrastructure.Repositories;

internal sealed class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<Subscription>> GetAllByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Subscription>()
                    .AsNoTracking()
                    .Where(x => x.OrganizationId == organizationId)
                    .Include(x => x.Addons)
                        .ThenInclude(a => a.LimitedFeatureAddon)
                    .ToListAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByIdWithAddonAsync(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Subscription>()
            .Include(x => x.Addons)
                .ThenInclude(a => a.LimitedFeatureAddon)
            .FirstOrDefaultAsync(x => x.Id == id &&
                x.Status == SubscriptionStatus.Active
            , cancellationToken);
    }

    public async Task<Subscription?> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Subscription>()
            .AsNoTracking()
            // Combine predicates into a single expression and pass the cancellation token.
            .FirstOrDefaultAsync(s =>
                s.OrganizationId == organizationId &&
                s.Status == SubscriptionStatus.Active
            , cancellationToken);
    }
}
