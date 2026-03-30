namespace Viora.Domain.Subscriptions;

public interface ISubscriptionRepository
{
    void Add(Subscription subscription);
    Task<Subscription> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
