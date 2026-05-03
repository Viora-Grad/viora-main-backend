using Viora.Domain.Orders;

namespace Viora.Infrastructure.Repositories;

internal class AddonOrderRepository : Repository<AddonOrder>, IAddonOrderRepository
{
    public AddonOrderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
