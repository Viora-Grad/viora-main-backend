namespace Viora.Domain.Orders;

public interface ISubscriptionOrderRepository
{
    public void Add(SubscriptionOrder order);
    Task<List<SubscriptionOrder>> GetAllByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken);
}
