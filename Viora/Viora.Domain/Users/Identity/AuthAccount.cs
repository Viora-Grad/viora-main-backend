using Viora.Domain.Abstractions;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users.Identity;

/// <summary>
/// Represents the authentication account for a user.
/// This aggregate tracks the account's email, verification state, current status,
/// last login timestamp, linked authentication identities, and assigned roles.
/// </summary>
/// <remarks>
/// Roles are reserved for future role-based access control support.
/// </remarks>
public class AuthAccount : Entity
{

    private readonly HashSet<Role> _roles = []; // for future role-based access control implementation
    private readonly List<AuthIdentity> _identities = [];
    private AuthAccount(Guid id, Email email, DateTime lastLoginAt, AccountStatus status)
        : base(id)
    {
        Email = email;
        LastLoginAt = lastLoginAt;
        Status = status;
    }
    private AuthAccount() { } // for ef core
    public Email Email { get; private set; } = null!;
    public DateTime LastLoginAt { get; private set; }
    public AccountStatus Status { get; private set; }
    public bool IsEmailVerified { get; private set; } = false;
    public IReadOnlyCollection<AuthIdentity> Identities => _identities.AsReadOnly();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    public void MarkAsDeleted()
    {
        Status = AccountStatus.Deleted;
    }
    public void Deactivate()
    {
        Status = AccountStatus.Deactivated;
    }
    public void Activate()
    {
        Status = AccountStatus.Active;
    }
    public void VerifyEmail()
    {
        IsEmailVerified = true;
    }
    public Result LinkIdentity(AuthIdentity identity)
    {
        if (identity is null)
            return Result.Failure(UserErrors.EmptyField);

        var exists = _identities.Any(i => i.Provider == identity.Provider.ToLower() && i.ProviderKey == identity.ProviderKey);
        if (exists)
            return Result.Failure(UserErrors.IdentityLinked);

        _identities.Add(identity);
        return Result.Success();
    }
    public void RecordLogin(DateTime utcNow)
    {
        LastLoginAt = utcNow;
    }
    public static AuthAccount Create(Email email, DateTime utcNow)
    {
        var user = new AuthAccount(Guid.NewGuid(), email, utcNow, AccountStatus.Active);
        user._roles.Add(Role.Registered); // default role assignment, can be changed later
        return user;
    }

}