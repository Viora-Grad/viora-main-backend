namespace Viora.Domain.Subscriptions;

public interface ISubscriptionRepository
{
    public void Add(Subscription subscription);
    public Task<Subscription> GetByIdWithAddonAsync(Guid id, CancellationToken cancellationToken);
    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task<Subscription?> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken);
    public Task<List<Subscription>> GetAllByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken);

}
