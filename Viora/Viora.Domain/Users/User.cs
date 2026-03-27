using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users;

// abstract class for different inheritence 
public abstract class User : Entity
{
    protected User(Guid id, FirstName firstName, LastName lastName, Email email, Contact contact, UserType userType)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Contact = contact;
        UserType = userType;
    }
    protected User() { } // for ef core
    public FirstName FirstName { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Contact Contact { get; private set; } = null!;
    public HashedPassword HashedPassword { get; private set; } = null!;
    public UserType UserType { get; private set; }
}
