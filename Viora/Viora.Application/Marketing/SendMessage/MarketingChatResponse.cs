namespace Viora.Application.Marketing.SendMessage;

// ContentPending=true means a Manus generation task was started for a MarketingContent turn; the client
// should poll the poll-content endpoint until the copy is ready. For a FinalizePost turn it is false and
// Reply/FacebookPostId carry the archived-post result.
public sealed record MarketingChatResponse(
    Guid ChatId,
    string Status,
    string Reply,
    string DetectedIntent,
    string? FacebookPostId,
    bool ContentPending);
