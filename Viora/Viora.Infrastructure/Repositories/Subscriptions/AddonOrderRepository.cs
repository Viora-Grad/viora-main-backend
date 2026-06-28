using Microsoft.EntityFrameworkCore;
using Viora.Domain.Orders;

namespace Viora.Infrastructure.Repositories.Subscriptions;

internal class AddonOrderRepository : Repository<AddonOrder>, IAddonOrderRepository
{
    public AddonOrderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<AddonOrder>> GetAllByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<AddonOrder>()
            .AsNoTracking()
            .Where(order => order.OrganizationId == organizationId)
            .Include(order => order.Addons)
            .OrderByDescending(order => order.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
