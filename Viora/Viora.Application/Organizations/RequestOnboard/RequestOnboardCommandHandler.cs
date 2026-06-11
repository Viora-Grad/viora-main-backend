using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Organizations.RequestOnboard;

// TODO add the legal papers submissions :/
internal class RequestOnboardCommandHandler(
    IOrganizationRepository organizationRepository,
    IOrganizationApplicationRepository applicationRepository,
    IUserRepository userRepository,
    IDateTimeProvider dateTimeProvider,
    IOnboardingSettings onboardingSettings,
    IUnitOfWork unitOfWork) : ICommandHandler<RequestOnboardCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RequestOnboardCommand request, CancellationToken cancellationToken)
    {
        var isUserExisting = await userRepository.IsUserExistent(request.OwnerId);
        if (!isUserExisting)
            throw new NotFoundException($"User with Id {request.OwnerId} does not eixst");

        bool isOrganizationExistingForOwner = await organizationRepository.IsOrganizationExistForOwnerAsync(request.OwnerId, cancellationToken);
        if (isOrganizationExistingForOwner)
            return Result.Failure<Guid>(OnboardingErrors.OwnerHasOrganization);


        bool isActiveApplicationSubmited = await applicationRepository.IsApplicationSubmittedForOwnerAsync(request.OwnerId, cancellationToken);
        if (isActiveApplicationSubmited)
            return Result.Failure<Guid>(OnboardingErrors.OwnerHasOrganizationRequest);

        var latestApplication = await applicationRepository.GetLatestApplicationForOwnerAsync(request.OwnerId, cancellationToken);
        if (latestApplication != null)
        {
            var isInCoolDownPeriod = latestApplication.IsInCoolDownPeriod(onboardingSettings, dateTimeProvider.UtcNow).Value;
            if (isInCoolDownPeriod)
                return Result.Failure<Guid>(OnboardingErrors.IsInCoolDownPeriod(latestApplication.ExpiryDateUtc.Add(onboardingSettings.CoolDownPeriod)));
        }

        var organization = await organizationRepository.GetByNameAsync(request.ProposedName, cancellationToken);
        var activeOrganizationRequest = await applicationRepository.GetActiveApplicationByOrganizationNameAsync(request.ProposedName, cancellationToken);
        if (organization != null || activeOrganizationRequest != null)
            return Result.Failure<Guid>(OnboardingErrors.NameAlreadyTaken);

        var applicationResult = OrganizationApplication.Create(
            request.OwnerId,
            request.CountryId,
            request.ProposedName,
            request.Letter,
            request.About,
            request.ServiceTypes.Select(ServiceType.FromValue).ToList(),
            request.ServiceDescription,
            request.ReferralSource,
            request.BillingEmail,
            request.SupportEmail,
            dateTimeProvider.UtcNow,
            onboardingSettings
            );

        if (applicationResult.IsFailure)
            return Result.Failure<Guid>(applicationResult.Error);

        var application = applicationResult.Value;

        applicationRepository.Add(application);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(application.Id);
    }
}
