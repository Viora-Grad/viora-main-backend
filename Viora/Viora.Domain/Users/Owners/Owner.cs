using Viora.Domain.Abstractions;
using Viora.Domain.Users.Events;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users.Owners;

public sealed class Owner : Entity
{
    public Guid NationalityId { get; private set; } = Guid.Empty;
    public PersonalInfo PersonalInfo { get; private set; } = null!;

    public AcceptedTerms AcceptedTerms { get; private set; } = AcceptedTerms.Empty; // not sure why this is included here not in organization, but leave it for now 
    public Guid GatewayCredentialsId { get; private set; } = Guid.Empty;
    public DateTime BecameOwnerAt { get; private set; }
    public AuthAccount AuthAccount { get; private set; } = null!; // navigation property for ef core

    private Owner() { } // for ef core
    private Owner(Guid id, Guid nationalityId,
        PersonalInfo personalInfo,
        Guid gatewayCredentialsId,
        DateTime becameOwnerAt,
        AcceptedTerms acceptedTerms)
        : base(id)
    {
        NationalityId = nationalityId;
        PersonalInfo = personalInfo;
        GatewayCredentialsId = gatewayCredentialsId;
        BecameOwnerAt = becameOwnerAt;
        AcceptedTerms = acceptedTerms;
    }

    public static Owner Create(Guid nationalityId, PersonalInfo personalInfo,
        Guid gatewayCredentialsId, DateTime utcNow, AcceptedTerms acceptedTerms)
    {
        // add any validation if needed
        Owner owner = new(Guid.NewGuid(), nationalityId, personalInfo, gatewayCredentialsId, utcNow, acceptedTerms);

        owner.RaiseDomainEvent(new OwnerCreatedEvent(owner.Id));

        return owner;
    }
    public Result UpdateTerms(DateTime utcNow)
    {
        AcceptedTerms = new AcceptedTerms(utcNow, AcceptedTerms.Version + 1);

        return Result.Success();
    }
}