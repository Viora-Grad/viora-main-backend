using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users;

public sealed class Owner : User
{
    public Nationality Nationality { get; private set; } = null!;
    public AcceptedTerms AcceptedTerms { get; private set; } = null!; // not sure why this is included here not in organization, but leave it for now 
    public Guid GatewayCredentialsId { get; private set; }

    private Owner() { } // for ef core
    private Owner(Guid id, FirstName firstName, LastName lastName, Email email,
        IEnumerable<Contact> contacts, HashedPassword hashedPassword,
        Nationality nationality, AcceptedTerms acceptedTerms, Guid gatewayCredentialsId)
        : base(id, firstName, lastName, email, contacts, hashedPassword, UserType.Owner)
    {
        Nationality = nationality;
        AcceptedTerms = acceptedTerms;
        GatewayCredentialsId = gatewayCredentialsId;
    }
    public static Owner Create(Guid id, FirstName firstName, LastName lastName, Email email,
        IEnumerable<Contact> contacts, HashedPassword hashedPassword,Nationality nationality, AcceptedTerms acceptedTerms, Guid gatewayCredentialsId)
    {
        return new Owner(id, firstName, lastName, email, contacts, hashedPassword, nationality, acceptedTerms, gatewayCredentialsId);
    }
    public Result UpdateTerms()
    {
        AcceptedTerms = new AcceptedTerms(DateTime.UtcNow, AcceptedTerms.Version + 1);

        return Result.Success();
    }
}