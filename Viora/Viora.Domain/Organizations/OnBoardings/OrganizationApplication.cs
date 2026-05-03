using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OnBoardings.Internals;
using Viora.Domain.Organizations.Shared;
using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Domain.Organizations.OnBoardings;

public sealed class OrganizationApplication : Entity
{
    public Guid OwnerId { get; private set; }
    public Guid CountryId { get; private set; }
    public Name ProposedName { get; set; } = default!;
    public Letter ApplicationLetter { get; private set; } = default!;

    public ServiceType ProposedServiceType { get; private set; } = default!;
    public ServiceDescription ServiceDescription { get; private set; } = default!;

    public DateTime SubmittedOnUtc { get; private set; }
    public ApplicationStatus Status { get; private set; }
    public ReferralSource ReferralSource { get; private set; }
    public Guid? RejectedBy { get; private set; }
    public DateTime ExpiryDateUtc { get; private set; }
    public BillingEmail BillingEmail { get; private set; } = default!;
    public SupportEmail SupportEmail { get; private set; } = default!;

    private OrganizationApplication() { } // for Ef


    public static Result<OrganizationApplication> Create(Guid ownerId,
        Guid countryId,
        Name proposedName,
        Letter applicationLetter,
        ServiceType proposedServiceType,
        ServiceDescription serviceDescription,
        ReferralSource referralSource,
        BillingEmail billingEmail,
        SupportEmail supportEmail,
        DateTime currentDateTime,
        IOnboardingSettings onboardingSettings)
    {
        var application = new OrganizationApplication
        {
            OwnerId = ownerId,
            CountryId = countryId,
            ProposedName = proposedName,
            ApplicationLetter = applicationLetter,
            ProposedServiceType = proposedServiceType,
            ServiceDescription = serviceDescription,
            ReferralSource = referralSource,
            BillingEmail = billingEmail,
            SupportEmail = supportEmail,
            Status = ApplicationStatus.Pending,
            SubmittedOnUtc = currentDateTime,
            ExpiryDateUtc = currentDateTime.AddDays(onboardingSettings.DaysTillExpiry)
        };
        return Result.Success(application);
    }
    public Result MarkAccepted(DateTime currentDateTime)
    {
        if (Status != ApplicationStatus.Pending)
            return Result.Failure(OnboardingErrors.StatusNotPending);

        if (ExpiryDateUtc < currentDateTime)
        {
            Status = ApplicationStatus.Expired;
            return Result.Failure(OnboardingErrors.AlreadyExpired);
        }
        Status = ApplicationStatus.Accepted;
        return Result.Success();
    }

    public Result<bool> IsInCoolDownPeriod(IOnboardingSettings onboardingSettings, DateTime utcNow)
    {
        return Result.Success(
            Status == ApplicationStatus.Rejected &&
            ExpiryDateUtc > utcNow &&
            ExpiryDateUtc <= utcNow.Add(onboardingSettings.CoolDownPeriod));
    }
}
