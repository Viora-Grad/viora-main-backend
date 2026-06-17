namespace Viora.Domain.Users.Identity;

public interface IIdentityRepository
{
    Task<AuthIdentity?> GetByProviderAsync(string provider, string providerKey, CancellationToken cancellationToken = default);
    Task<List<AuthIdentity>?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(AuthIdentity authIdentity);
}
