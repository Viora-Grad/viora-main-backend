using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails.Internal;
using Viora.Domain.Organizations.Shared;
using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Domain.Organizations.OrganizationDetails;

public sealed class Organization : Entity
{
    public Guid OwnerId { get; private set; }
    public Guid CountryId { get; private set; }
    public Guid? LogoId { get; private set; }
    public Name Name { get; private set; } = default!;
    public About About { get; private set; } = default!;

    public HashSet<ServiceType> ServicesProvided { get; private set; } = [];
    public ServiceDescription ServiceDescription { get; private set; } = default!;
    public DateTime JoinedOnUtc { get; private set; }
    public OrganizationStatus Status { get; private set; }
    public ReferralSource ReferralSource { get; private set; }
    public OrganizationRating Rating { get; private set; } = default!;

    public BillingEmail BillingEmail { get; private set; } = default!;
    public SupportEmail SupportEmail { get; private set; } = default!;

    private Organization(
        Guid id,
        Guid ownerId,
        Guid countryId,
        Name name,
        About about,
        ServiceDescription description,
        ICollection<ServiceType> serviceTypes,
        DateTime joinedOnUtc,
        ReferralSource referralSource,
        BillingEmail billingEmail,
        SupportEmail supportEmail) : base(id)
    {
        OwnerId = ownerId;
        CountryId = countryId;
        Name = name;
        About = about;
        ServiceDescription = description;
        ServicesProvided = [.. serviceTypes];
        JoinedOnUtc = joinedOnUtc;
        ReferralSource = referralSource;
        BillingEmail = billingEmail;
        SupportEmail = supportEmail;
    }
    private Organization() { }  // for EfCore

    public static Result<Organization> Create(
        Guid ownerId,
        Guid countryId,
        string name,
        string about,
        string serviceDescription,
        ICollection<ServiceType> serviceTypes,
        DateTime joinedOnUtc,
        ReferralSource referralSource,
        string billingEmail,
        string supportEmail)
    {
        var organization = new Organization(Guid.NewGuid(), ownerId, countryId, name, about, serviceDescription, serviceTypes, joinedOnUtc, referralSource, billingEmail, supportEmail)
        {
            Status = OrganizationStatus.Active
        };

        return Result.Success(organization);
    }

    public Result Hide()
    {
        if (Status != OrganizationStatus.Active)
            return Result.Failure(OrganizationErrors.OrganizationMustBeActiveToHide);

        Status = OrganizationStatus.Hidden;
        return Result.Success();
    }

    public Result Suspend()
    {
        Status = OrganizationStatus.Suspended;
        return Result.Success();
    }
}