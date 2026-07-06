namespace Viora.Domain.Marketing;

// Facebook Graph API configuration. Per-tenant Page tokens are stored per organization, not here.
public interface IMetaSettings
{
    public string BaseUrl { get; set; }
    public string GraphApiVersion { get; set; }

    // Facebook App credentials used to exchange a short-lived user token for a long-lived one
    // (GET /oauth/access_token?grant_type=fb_exchange_token). The secret must be supplied via env, never committed.
    public string AppId { get; set; }
    public string AppSecret { get; set; }
}
