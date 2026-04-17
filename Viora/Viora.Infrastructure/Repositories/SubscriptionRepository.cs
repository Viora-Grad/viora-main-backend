using Microsoft.EntityFrameworkCore;
using Viora.Domain.Subscriptions;

namespace Viora.Infrastructure.Repositories;

internal sealed class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Subscription?> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Subscription>()
             .AsNoTracking()
             .FirstOrDefaultAsync(x =>
                 x.OrganizationId == organizationId,
                 cancellationToken);
    }
}
