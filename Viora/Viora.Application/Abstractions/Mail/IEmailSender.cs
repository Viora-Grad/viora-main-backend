namespace Viora.Application.Abstractions.Mail;

public interface IEmailSender
{
    public Task<bool> SendAsync(string emailId, EmailMessage mail, CancellationToken cancellationToken = default);
}
