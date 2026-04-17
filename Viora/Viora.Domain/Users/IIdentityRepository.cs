namespace Viora.Domain.Users;

public interface IIdentityRepository
{
    Task<AuthIdentity?> GetByProviderAsync(string provider, string providerKey, CancellationToken cancellationToken = default);
    Task<AuthIdentity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(AuthIdentity authIdentity);
}
