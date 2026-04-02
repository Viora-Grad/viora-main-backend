using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OnBoardings.Internals;
using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Domain.Organizations.OnBoardings;

public sealed class OrganizationApplication : Entity
{
    public Guid OwnerId { get; private set; }
    public Guid CountryId { get; private set; }
    public Name ProposedName { get; set; } = default!;
    public Letter ApplicationLetter { get; private set; } = default!;
    public ServiceType ProposedServiceType { get; private set; } = default!;
    public ApplicationStatus Status { get; private set; }
    public ReferralSource ReferralSource { get; private set; }
    public Guid? RejectedBY { get; private set; }
    public DateOnly ExpiryDateUtc { get; private set; }

    private OrganizationApplication() { } // for Ef


}
