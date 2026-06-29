using Microsoft.EntityFrameworkCore;
using Viora.Domain.Orders;

namespace Viora.Infrastructure.Repositories.Subscriptions;

internal class AddonOrderRepository : Repository<AddonOrder>, IAddonOrderRepository
{
    public AddonOrderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    // Tracked + includes Addons so a paid webhook can both mutate the order and read its addon ids.
    public override async Task<AddonOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<AddonOrder>()
            .Include(order => order.Addons)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
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
