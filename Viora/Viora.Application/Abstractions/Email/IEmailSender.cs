namespace Viora.Application.Abstractions.Email;

public interface IEmailSender
{
    public Task Send(string emailId, EmailMessage mail, CancellationToken cancellationToken = default);
}
