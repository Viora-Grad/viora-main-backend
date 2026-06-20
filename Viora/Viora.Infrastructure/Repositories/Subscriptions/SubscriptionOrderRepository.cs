using Viora.Domain.Orders;

namespace Viora.Infrastructure.Repositories.Subscriptions;

internal class SubscriptionOrderRepository : Repository<SubscriptionOrder>, ISubscriptionOrderRepository
{
    public SubscriptionOrderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
