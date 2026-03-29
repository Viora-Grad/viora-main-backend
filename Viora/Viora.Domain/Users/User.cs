using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users;

// abstract class for different inheritence 
public abstract class User : Entity
{
    private readonly HashSet<Contact> _contact = [];
    protected User(Guid id, FirstName firstName, LastName lastName, Email email, IEnumerable<Contact> contact, HashedPassword hashedPassword, UserType userType)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        _contact = [.. contact];
        HashedPassword = hashedPassword;
        UserType = userType;
    }
    protected User() { } // for ef core
    public FirstName FirstName { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public HashedPassword HashedPassword { get; private set; } = null!;
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
    public Result ChangePassword(HashedPassword newHashedPassword)
    {
        if (newHashedPassword is null)
            return Result.Failure(UserErrors.EmptyField);

        HashedPassword = newHashedPassword;
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
