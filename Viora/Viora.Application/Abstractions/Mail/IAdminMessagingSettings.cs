namespace Viora.Application.Abstractions.Mail;

public interface IAdminMessagingSettings
{
    public IReadOnlyList<string> Emails { get; set; }
}
