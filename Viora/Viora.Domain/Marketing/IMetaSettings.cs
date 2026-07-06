namespace Viora.Domain.Marketing;

// Facebook Graph API configuration. Per-tenant Page tokens are stored per organization, not here.
public interface IMetaSettings
{
    public string BaseUrl { get; set; }
    public string GraphApiVersion { get; set; }
}
