using Viora.Domain.Vivi.Marketing;

namespace Viora.Infrastructure.Settings;

public class ViviMarketingSettings : IViviMarketingSettings
{
    public string MarketingApiKey { get; set; } = default!;
    public string MarketingModel { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
}
