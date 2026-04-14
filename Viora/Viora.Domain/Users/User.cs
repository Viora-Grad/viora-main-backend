using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users;

/// <summary>
/// User entity represents the identity of the different actors in the system, such as customers and owners.
/// It contains common properties and behaviors that are shared among all user types.
/// <strong>Might default the user to be a Customer entity as a default system assignment</strong>
/// </summary>
public class User : Entity
{
    private readonly HashSet<Contact> _contact = [];
    // TODO: private readonly HashSet<Role> _roles = []; // for future role-based access control implementation
    private readonly List<AuthIdentity> _identities = [];
    protected User(Guid id, FirstName firstName, LastName lastName, UserName userName, Email email, Age age)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        Email = email;
        Age = age;
    }
    private User() { } // for ef core
    public FirstName FirstName { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;
    public UserName UserName { get; private set; } = null!; // based on mobile application request {Unique}
    public Email Email { get; private set; } = null!;
    public Age Age { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; } // for soft delete implementation
    public bool IsEmailVerified { get; private set; }
    public IReadOnlyList<Contact> Contact => _contact.ToList().AsReadOnly();
    public IReadOnlyCollection<AuthIdentity> Identities => _identities.AsReadOnly();

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
        IsDeleted = true;
        IsActive = false;
    }
    public void Deactivate()
    {
        IsActive = false;
    }
    public void Activate()
    {
        IsActive = true;
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
    public static User Create(FirstName firstName, LastName lastName, UserName userName, Email email, Age age)
    {
        return new User(Guid.NewGuid(), firstName, lastName, userName, email, age);

    }


}
