using Viora.Application.Abstractions.Mail;

namespace Viora.Infrastructure.Settings;

public class AdminMessagingSettings : IAdminMessagingSettings
{
    public IReadOnlyList<string> Emails { get; set; } = default!;
}
