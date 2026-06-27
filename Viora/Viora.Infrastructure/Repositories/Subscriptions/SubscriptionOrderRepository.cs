using Microsoft.EntityFrameworkCore;
using Viora.Domain.Orders;

namespace Viora.Infrastructure.Repositories.Subscriptions;

internal class SubscriptionOrderRepository : Repository<SubscriptionOrder>, ISubscriptionOrderRepository
{
    public SubscriptionOrderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<SubscriptionOrder>> GetAllByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<SubscriptionOrder>()
            .AsNoTracking()
            .Where(order => order.OrganizationId == organizationId)
            .OrderByDescending(order => order.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
