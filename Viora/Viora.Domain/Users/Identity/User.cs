using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users.Identity;

/// <summary>
/// User entity represents the identity of the different actors in the system, such as customers and owners.
/// It contains common properties and behaviors that are shared among all user types.
/// <strong>Might default the user to be a Customer entity as a default system assignment</strong>
/// </summary>
public class User : Entity
{
    private readonly HashSet<Contact> _contact = [];
    private readonly HashSet<Role> _roles = []; // for future role-based access control implementation
    private readonly List<AuthIdentity> _identities = [];
    protected User(Guid id, FirstName firstName, LastName lastName, UserName userName, Email email,
        Age age, UserStatus status, DateTime utcNow)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        Email = email;
        Age = age;
        Status = status;
        JoinedAt = utcNow;
    }
    private User() { } // for ef core
    public FirstName FirstName { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;
    public UserName UserName { get; private set; } = null!; // based on mobile application request {Unique}
    public Email Email { get; private set; } = null!;
    public Age Age { get; private set; } = null!;
    public DateTime JoinedAt { get; private set; }
    public UserStatus Status { get; private set; }
    public bool IsEmailVerified { get; private set; } = false;
    public IReadOnlyList<Contact> Contacts => _contact.ToList().AsReadOnly();
    public IReadOnlyCollection<AuthIdentity> Identities => _identities.AsReadOnly();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    public void AddContact(Contact contact)
    {
        // might trigger domain events for contact change
        _contact.Add(contact);
    }
    public void RemoveContact(Contact contact)
    {
        _contact.Remove(contact);
    }
    public Result UpdateName(FirstName firstName, LastName lastName)
    {
        // might trigger domain events for name change
        if (firstName is null || lastName is null)
            return Result.Failure(UserErrors.EmptyField);
        FirstName = firstName;
        LastName = lastName;
        return Result.Success();
    }
    public Result SetContacts(IEnumerable<Contact> contacts)
    {
        // might trigger domain events for contact change
        if (contacts is null)
            return Result.Failure(UserErrors.EmptyField);

        _contact.Clear();
        _contact.UnionWith(contacts);
        return Result.Success();
    }
    public Result UpdateEmail(Email newEmail)
    {
        // might trigger domain events for email change
        if (newEmail is null)
            return Result.Failure(UserErrors.EmptyField);

        Email = newEmail;
        return Result.Success();
    }
    public void MarkAsDeleted()
    {
        Status = UserStatus.Deleted;
    }
    public void Deactivate()
    {
        Status = UserStatus.Deactivated;
    }
    public void Activate()
    {
        Status = UserStatus.Active;
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
    public static User Create(FirstName firstName, LastName lastName, UserName userName, Email email, Age age, DateTime utcNow)
    {

        var user = new User(Guid.NewGuid(), firstName, lastName, userName, email, age, UserStatus.Active, utcNow);
        user._roles.Add(Role.Registered); // default role assignment, can be changed later
        return user;
    }

}