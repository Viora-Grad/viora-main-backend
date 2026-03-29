using Viora.Domain.Abstractions;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users;

public sealed class Owner : User
{
    public Nationality Nationality { get; private set; } = null!;
    public AcceptedTerms AcceptedTerms { get; private set; } = AcceptedTerms.Empty; // not sure why this is included here not in organization, but leave it for now 
    public Guid GatewayCredentialsId { get; private set; } = Guid.Empty;

    private Owner() { } // for ef core
    private Owner(Guid id, FirstName firstName, LastName lastName, Email email, Nationality nationality)
        : base(id, firstName, lastName, email, UserType.Owner)
    {
        Nationality = nationality;
    }
    public static Owner Create(Guid id, FirstName firstName, LastName lastName, Email email, Nationality nationality)
    {
        return new Owner(id, firstName, lastName, email, nationality);
    }
    public Result SetGatewayCredentialsId(Guid gatewayCredentialsId)
    {
        if (gatewayCredentialsId == Guid.Empty)
            return Result.Failure(UserErrors.EmptyField);

        GatewayCredentialsId = gatewayCredentialsId;
        return Result.Success();
    }
    public Result UpdateTerms()
    {
        AcceptedTerms = new AcceptedTerms(DateTime.UtcNow, AcceptedTerms.Version + 1);

        return Result.Success();
    }
}