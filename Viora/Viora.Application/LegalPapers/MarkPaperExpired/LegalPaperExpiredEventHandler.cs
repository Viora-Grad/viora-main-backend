using MediatR;
using Viora.Application.Abstractions.Mail;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.LegalPapers;
using Viora.Domain.Organizations.LegalPapers.Events;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Users.Identity;

namespace Viora.Application.LegalPapers.MarkPaperExpired;

internal sealed class LegalPaperExpiredEventHandler(
    ILegalPaperRepository legalPaperRepository,
    IOrganizationApplicationRepository applicationRepository,
    IUserRepository userRepository,
    IEmailSender emailSender,
    IAdminMessagingSettings adminMessagingSettings,
    IUnitOfWork unitOfWork) : INotificationHandler<LegalPaperExpiredDomainEvent>
{
    public async Task Handle(LegalPaperExpiredDomainEvent notification, CancellationToken cancellationToken)
    {
        var legalPaper = await legalPaperRepository.GetByIdAsync(notification.paperId, cancellationToken)
            ?? throw new InvalidOperationException("Paper not found");

        var application = await applicationRepository.GetByIdAsync(legalPaper.ApplicationId, cancellationToken)
            ?? throw new InvalidOperationException("application not found");

        var user = await userRepository.GetByIdAsync(application.OwnerId, cancellationToken)
            ?? throw new InvalidOperationException("Owner not found");

        legalPaper.MarkExpired();

        var Tasks = new List<Task>
        {
            emailSender.SendAsync(user.Email.Value,
            EmailTemplateFactory.LegalPaperExpiredClient(
                user.PersonalInfo.FirstName,
                legalPaper.Name.Value,
                legalPaper.ExpiryDateUtc),
            cancellationToken)
        };

        foreach (var email in adminMessagingSettings.Emails)
        {
            Tasks.Add(
                emailSender.SendAsync(email,
                EmailTemplateFactory.LegalPaperExpiredAdmin(
                    application.ProposedName,
                    user.PersonalInfo.FirstName,
                    legalPaper.Name.Value,
                    legalPaper.ExpiryDateUtc),
                cancellationToken));
        }

        await Task.WhenAll(Tasks);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
