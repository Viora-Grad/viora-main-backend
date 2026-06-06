using Viora.Domain.Vivi.Health;

namespace Viora.Infrastructure.Settings;

public class ViviHealthSettings : IViviHealthSettings
{
    public string HealthChatApiKey { get; set; } = default!;
    public string HealthModel { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
}
