using Viora.Domain.Shared;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users;

public sealed class Customer : User
{
    public int Age { get; private set; }
    public List<Guid> OrganizationsRegistered { get; private set; } = [];
    public Guid? MedicalRecordId { get; private set; } // can be removed since the relation is optional from the customer side

    private Customer() { } // for ef core
    private Customer(Guid id, FirstName firstName, LastName lastName, Email email, Contact contact, HashedPassword hashedPassword, int age)
        : base(id, firstName, lastName, email, contact, hashedPassword, UserType.Customer)
    {
        Age = age;
    }
    public static Customer Create(Guid id, FirstName firstName, LastName lastName, Email email, Contact contact, HashedPassword hashedPassword, int age)
    {
        // add any validation if needed
        return new Customer(id, firstName, lastName, email, contact, hashedPassword, age);
    }

}
