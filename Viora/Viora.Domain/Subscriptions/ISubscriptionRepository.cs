namespace Viora.Domain.Subscriptions;

public interface ISubscriptionRepository
{
    public void Add(Subscription subscription);
    public Task<Subscription> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task<Subscription> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken);

}
