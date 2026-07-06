namespace Viora.Api.Controllers.Marketing;

public sealed record SaveMetaCredentialRequest(string PageId, string AccessToken);

// Facebook Login connect flow: AuthCode is the short-lived user token (fb_exchange_token) from the client SDK.
public sealed record ConnectMetaPageRequest(string AuthCode, string PageId);

public sealed record StartChatRequest(string? FirstMessage);

public sealed record SendMarketingMessageRequest(string Message);
