using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using Viora.Application.Abstractions.Mail;

namespace Viora.Infrastructure.Mail;

internal sealed class EmailSender(IEmailSettings emailSettings, ILogger<EmailSender> logger) : IEmailSender
{
    public async Task<bool> SendAsync(string emailId, EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
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
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {EmailId}.", emailId);
            return false;
        }
    }
}
