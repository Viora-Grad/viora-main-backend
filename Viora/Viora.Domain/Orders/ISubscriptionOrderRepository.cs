namespace Viora.Domain.Orders;

public interface ISubscriptionOrderRepository
{
    public void Add(SubscriptionOrder order);
    Task<SubscriptionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SubscriptionOrder>> GetAllByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken);
}
