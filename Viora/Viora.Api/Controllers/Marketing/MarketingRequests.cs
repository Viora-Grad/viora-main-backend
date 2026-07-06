namespace Viora.Api.Controllers.Marketing;

public sealed record SaveMetaCredentialRequest(string PageId, string AccessToken);

public sealed record StartChatRequest(string? FirstMessage);

public sealed record SendMarketingMessageRequest(string Message);
