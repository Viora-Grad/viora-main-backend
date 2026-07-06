using Viora.Domain.Marketing;

namespace Viora.Infrastructure.Settings;

public class ManusSettings : IManusSettings
{
    public string BaseUrl { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
}
