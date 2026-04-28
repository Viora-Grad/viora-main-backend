using Viora.Domain.Abstractions;

namespace Viora.Domain.Users.Identity;
/// <summary>
/// An Entity that represents the authentication identity of a user,
/// which can be linked to multiple users for different authentication providers (e.g., email/password, social logins).
/// </summary>
/// <remarks><strong>Opting for not using Value Objects to not over complicate things</strong></remarks>
public sealed class AuthIdentity : Entity
{
    private AuthIdentity(Guid id, string provider, Guid userId, string providerKey, DateTime utcNow)
        : base(id)
    {
        Provider = provider;
        UserId = userId;
        ProviderKey = providerKey;
        CreatedAt = utcNow;
    }
    private AuthIdentity() { } // for ef core
    public string Provider { get; private set; } = null!;
    public string ProviderKey { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public User User { get; private set; } = null!;

    public static AuthIdentity Create(string provider, Guid userId, string providerKey, DateTime utcNow)
    {
        return new AuthIdentity(Guid.NewGuid(), provider.ToLower().Trim(), userId, providerKey, utcNow);
    }
    public void RecordLogin(DateTime utcNow)
    {
        LastLoginAt = utcNow;
    }
}
