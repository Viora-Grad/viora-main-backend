using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails.Internal;
using Viora.Domain.Organizations.Shared;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Shared;

namespace Viora.Domain.Organizations.OrganizationDetails;

public sealed class Organization : Entity
{
    public Guid OwnerId { get; private set; }
    public Guid CountryId { get; private set; }
    public Guid? LogoId { get; private set; }
    public Name Name { get; private set; } = default!;
    public About About { get; private set; } = default!;

    public List<ServiceType> ServicesProvided { get; private set; } = [];
    public ServiceDescription ServiceDescription { get; private set; } = default!;
    public DateTime JoinedOnUtc { get; private set; }
    public OrganizationStatus Status { get; private set; }
    public ReferralSource ReferralSource { get; private set; }
    public OrganizationRating Rating { get; private set; } = new(0, 0.0);

    public BillingEmail BillingEmail { get; private set; } = default!;
    public SupportEmail SupportEmail { get; private set; } = default!;

    public SubDomain SubDomain { get; private set; } = Guid.NewGuid().ToString();

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
        ServicesProvided = serviceTypes.Distinct().ToList();
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

    public Result UpdateLogo(Guid mediaId)
    {
        if (Status != OrganizationStatus.Active)
            return Result.Failure(OrganizationErrors.OrganizationMustBeActiveToUpdateLogo);

        LogoId = mediaId;
        return Result.Success();
    }

    // Replaces the editable profile fields in one atomic change.
    public Result UpdateProfile(
        string subDomain,
        string supportEmail,
        string billingEmail,
        string serviceDescription,
        ICollection<ServiceType> servicesProvided,
        string about)
    {
        if (string.IsNullOrWhiteSpace(subDomain) || subDomain.Any(char.IsWhiteSpace))
            return Result.Failure(OrganizationErrors.InvalidSubDomain);

        if (servicesProvided is null || servicesProvided.Count == 0)
            return Result.Failure(OrganizationErrors.NoServicesProvided);

        SubDomain = subDomain;
        SupportEmail = supportEmail;
        BillingEmail = billingEmail;
        ServiceDescription = serviceDescription;
        ServicesProvided = servicesProvided.Distinct().ToList();
        About = about;

        return Result.Success();
    }

    public Result UpdateRating(int ratingOutOfTen)
    {
        if (ratingOutOfTen > 10 || ratingOutOfTen < 0)
            return Result.Failure(OrganizationErrors.RatingOutOfBound);

        var count = Rating.Count + 1;
        var newRating = ((Rating.AverageOutOfTen * Rating.Count) + ratingOutOfTen) / count;

        Rating = new(count, newRating);
        return Result.Success();
    }

}