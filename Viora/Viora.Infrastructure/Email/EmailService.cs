using System.Net;
using System.Net.Mail;
using Viora.Application.Abstractions.Email;

namespace Viora.Infrastructure.Email;

internal sealed class EmailService(IEmailSettings emailSettings) : IEmailSender
{
    public async Task Send(string emailId, EmailMessage message, CancellationToken cancellationToken = default)
    {
        using var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(emailSettings.Email);
        mailMessage.To.Add(emailId);
        mailMessage.Subject = message.Header;
        mailMessage.Body = message.Body;
        mailMessage.IsBodyHtml = true;

        using var smtp = new SmtpClient(emailSettings.SmtpDomain, emailSettings.DomainPort);
        smtp.Credentials = new NetworkCredential(emailSettings.Email, emailSettings.Password);
        smtp.EnableSsl = true;

        await smtp.SendMailAsync(mailMessage, cancellationToken);
    }
}
