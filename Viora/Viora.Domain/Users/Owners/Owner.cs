using Viora.Domain.Abstractions;
using Viora.Domain.Users.Events;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users.Owners;

public sealed class Owner : Entity
{
    public Guid NationalityId { get; private set; } = Guid.Empty;
    public PersonalInfo PersonalInfo { get; private set; } = null!;
    public DateTime BecameOwnerAt { get; private set; }
    public User UserProfile { get; private set; } = null!; // navigation property for ef core

    private Owner() { } // for ef core
    private Owner(Guid id, Guid nationalityId, PersonalInfo personalInfo, DateTime becameOwnerAt)
        : base(id)
    {
        NationalityId = nationalityId;
        PersonalInfo = personalInfo;
        BecameOwnerAt = becameOwnerAt;
    }

    public static Owner Create(Guid id, Guid nationalityId, PersonalInfo personalInfo, DateTime utcNow) // id will be the same as User entity
    {
        // add any validation if needed
        Owner owner = new(id, nationalityId, personalInfo, utcNow);

        owner.RaiseDomainEvent(new OwnerCreatedEvent(owner.Id));

        return owner;
    }
}