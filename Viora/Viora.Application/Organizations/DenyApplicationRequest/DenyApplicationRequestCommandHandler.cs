using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Organizations.DenyApplicationRequest;

internal class DenyApplicationRequestCommandHandler(
    IOrganizationApplicationRepository applicationRepositroy,
    IEmailSender emailSender,
    IUserRepository userRepository,
    IOnboardingSettings onboardingSettings,
    IUnitOfWork unitOfWork) : ICommandHandler<DenyApplicationRequestCommand>
{
    public async Task<Result> Handle(DenyApplicationRequestCommand request, CancellationToken cancellationToken)
    {
        var application = await applicationRepositroy.GetByIdAsync(request.ApplicationId) ?? throw new NotFoundException("Application Not Found");
        var result = application.Deny(request.RejectedById);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        var owner = await userRepository.GetByIdAsync(application.OwnerId, cancellationToken) ?? throw new NotFoundException($"Application owner:{application.OwnerId} Not Found");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await emailSender.SendAsync(owner.Email.Value, EmailTemplateFactory.ApplicationDenied(owner.PersonalInfo.FirstName, application.ProposedName, onboardingSettings.CoolDownPeriod), cancellationToken);
        return Result.Success();
    }
}
