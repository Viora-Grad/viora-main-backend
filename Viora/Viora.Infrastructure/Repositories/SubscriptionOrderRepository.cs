using Viora.Domain.Orders;

namespace Viora.Infrastructure.Repositories;

internal class SubscriptionOrderRepository : Repository<SubscriptionOrder>, ISubscriptionOrderRepository
{
    public SubscriptionOrderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
