using Viora.Domain.Abstractions;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users.Identity;

/// <remarks>
/// `User` is intentionally not a base class for `Customer`/`Owner`.
/// We use composition instead of inheritance because identity/authentication is a separate concern
/// from business personas. A single account may legitimately be both `Customer` and `Owner`,
/// which inheritance models poorly (and C# has no multiple class inheritance).
/// Keeping them separate also preserves clear aggregate boundaries, avoids tight coupling of lifecycle rules,
/// and leaves room for future role/permission expansion (for example tenant-scoped staff authorization)
/// without forcing persona types into an inheritance hierarchy.
/// </remarks>

public class User : Entity
{

    private readonly HashSet<Role> _roles = []; // for future role-based access control implementation
    private readonly List<AuthIdentity> _identities = [];
    private User(Guid id, PersonalInfo personalInfo, Email email, DateTime createdAt, AccountStatus status)
        : base(id)
    {
        PersonalInfo = personalInfo;
        Email = email;
        CreatedAt = createdAt;
        Status = status;
    }
    private User() { } // for ef core
    public PersonalInfo PersonalInfo { get; set; } = null!;
    public Email Email { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
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
    public static User Create(PersonalInfo personalInfo, Email email, DateTime utcNow)
    {
        var user = new User(Guid.NewGuid(), personalInfo, email, utcNow, AccountStatus.Active);
        user._roles.Add(Role.Registered); // default role assignment, can be changed later
        return user;
    }

}