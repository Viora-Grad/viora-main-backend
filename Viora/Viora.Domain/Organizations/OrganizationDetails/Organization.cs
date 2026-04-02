using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails.Internal;
using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Domain.Organizations.OrganizationDetails;

public sealed class Organization : Entity
{
    public Guid OwnerId { get; private set; }
    public Guid CountryId { get; private set; }
    public Name Name { get; private set; } = default!;
    public ServiceSpecification ServiceSpecification { get; private set; } = default!;
    public DateOnly JoinedOnUtc { get; private set; }
    public OrganizationStatus Status { get; private set; }
    public ReferralSource ReferralSource { get; private set; }

    public BillingEmail? BillingEmail { get; private set; }
    public SupportEmail? SupportEmail { get; private set; }

}