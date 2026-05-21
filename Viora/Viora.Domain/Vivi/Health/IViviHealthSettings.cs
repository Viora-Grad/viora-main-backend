namespace Viora.Domain.Vivi.Health;

public interface IViviHealthSettings
{
    public string HealthChatApiKey { get; set; }
    public string HealthModel { get; set; }
    public string BaseUrl { get; set; }
}
