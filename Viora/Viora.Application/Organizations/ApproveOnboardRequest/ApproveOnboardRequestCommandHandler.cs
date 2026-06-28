using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Owners;

namespace Viora.Application.Organizations.ApproveOnboardRequest;

internal class ApproveOnboardRequestCommandHandler(
    IOrganizationApplicationRepository applicationRepository,
    IOrganizationRepository organizationRepository,
    IOwnerRepository ownerRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IEmailSender emailSender) : ICommandHandler<ApproveOnboardRequestCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ApproveOnboardRequestCommand request, CancellationToken cancellationToken)
    {
        var application = await applicationRepository.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new NotFoundException($"Organization application with ID {request.RequestId} not found.");

        var markResult = application.MarkAccepted(dateTimeProvider.UtcNow);
        if (markResult.IsFailure)
            return Result.Failure<Guid>(markResult.Error);

        var user = await userRepository.GetByIdAsync(application.OwnerId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {application.OwnerId} not found.");

        var existingOwner = await ownerRepository.GetByIdAsync(application.OwnerId, cancellationToken);
        if (existingOwner is not null)
            return Result.Failure<Guid>(UserErrors.AlreadyOwner);

        var ownerRole = await userRepository.FindRoleAsync(Role.Owner.Id, cancellationToken)
            ?? throw new InvalidOperationException("Owner role is not seeded in the database.");

        var promoteResult = user.PromoteToOwner(ownerRole);
        if (promoteResult.IsFailure)
            return Result.Failure<Guid>(promoteResult.Error);

        var owner = Owner.Create(user.Id, application.CountryId, user.PersonalInfo, dateTimeProvider.UtcNow);
        ownerRepository.Add(owner);

        var orgResult = Organization.Create(
            application.OwnerId,
            application.CountryId,
            application.ProposedName,
            application.About,
            application.ServiceDescription,
            application.ProposedServicesType,
            dateTimeProvider.UtcNow,
            application.ReferralSource,
            application.BillingEmail,
            application.SupportEmail);

        if (orgResult.IsFailure)
            return Result.Failure<Guid>(orgResult.Error);

        organizationRepository.Add(orgResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailSender.SendAsync(
            owner.UserProfile.Email.Value,
            EmailTemplateFactory.ApplicationAccepted($"{owner.PersonalInfo.FirstName} {owner.PersonalInfo.LastName}"),
            cancellationToken);

        return Result.Success(orgResult.Value.Id);
    }
}
