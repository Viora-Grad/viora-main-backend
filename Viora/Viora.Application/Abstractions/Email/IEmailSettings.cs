namespace Viora.Application.Abstractions.Email;

public interface IEmailSettings
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string SmtpDomain { get; set; }
    public int DomainPort { get; set; }
}
