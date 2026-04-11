using Viora.Domain.Abstractions;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users;

public sealed class Owner : Entity
{
    public Guid NationalityId { get; private set; } = Guid.Empty;
    public Guid UserId { get; private set; } // required and Unique 

    public AcceptedTerms AcceptedTerms { get; private set; } = AcceptedTerms.Empty; // not sure why this is included here not in organization, but leave it for now 
    public Guid GatewayCredentialsId { get; private set; } = Guid.Empty;

    private Owner() { } // for ef core
    private Owner(Guid id, Guid userId, Guid nationalityId, Guid gatewayCredentialsId, AcceptedTerms acceptedTerms)
        : base(id)
    {
        UserId = userId;
        NationalityId = nationalityId;
        GatewayCredentialsId = gatewayCredentialsId;
        AcceptedTerms = acceptedTerms;
    }
    public static Owner Create(Guid nationalityId, Guid userId, Guid gatewayCredentialsId, AcceptedTerms acceptedTerms)
    {
        // add any validation if needed
        return new Owner(Guid.NewGuid(), nationalityId, userId, gatewayCredentialsId, acceptedTerms);
    }
    public Result UpdateTerms()
    {
        AcceptedTerms = new AcceptedTerms(DateTime.UtcNow, AcceptedTerms.Version + 1);

        return Result.Success();
    }
}