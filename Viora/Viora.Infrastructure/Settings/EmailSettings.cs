using Viora.Application.Abstractions.Email;

namespace Viora.Infrastructure.Settings;

public class EmailSettings : IEmailSettings
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string SmtpDomain { get; set; } = default!;
    public int DomainPort { get; set; }
}
