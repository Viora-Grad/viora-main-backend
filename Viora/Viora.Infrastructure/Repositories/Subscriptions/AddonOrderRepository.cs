using Viora.Domain.Orders;

namespace Viora.Infrastructure.Repositories.Subscriptions;

internal class AddonOrderRepository : Repository<AddonOrder>, IAddonOrderRepository
{
    public AddonOrderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
