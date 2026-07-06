namespace Viora.Domain.Marketing;

// Manus marketing-content API configuration. Secret (ApiKey) comes from the environment at runtime.
public interface IManusSettings
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
}
