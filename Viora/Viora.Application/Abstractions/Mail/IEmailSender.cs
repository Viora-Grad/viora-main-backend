namespace Viora.Application.Abstractions.Mail;

public interface IEmailSender
{
    public Task Send(string emailId, EmailMessage mail, CancellationToken cancellationToken = default);
}
