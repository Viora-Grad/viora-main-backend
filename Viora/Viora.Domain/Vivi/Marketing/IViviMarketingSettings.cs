namespace Viora.Domain.Vivi.Marketing;

public interface IViviMarketingSettings
{
    public string MarketingApiKey { get; set; }
    public string MarketingModel { get; set; }
    public string BaseUrl { get; set; }
}