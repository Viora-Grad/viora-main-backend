using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users;

// abstract class for different inheritence 
public abstract class User : Entity
{
    private readonly HashSet<Contact> _contact = [];
    protected User(Guid id, FirstName firstName, LastName lastName, Email email, UserType userType)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UserType = userType;
    }
    protected User() { } // for ef core
    public FirstName FirstName { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public HashedPassword HashedPassword { get; private set; } = HashedPassword.Empty;
    public UserType UserType { get; private set; }
    public IReadOnlyList<Contact> Contact => _contact.ToList().AsReadOnly();

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


}
