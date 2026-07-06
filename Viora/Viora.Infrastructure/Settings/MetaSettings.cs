using Viora.Domain.Marketing;

namespace Viora.Infrastructure.Settings;

public class MetaSettings : IMetaSettings
{
    public string BaseUrl { get; set; } = "https://graph.facebook.com";
    public string GraphApiVersion { get; set; } = "v25.0";
}
