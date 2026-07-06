namespace Viora.Application.Marketing.GetChat;

public sealed record MarketingChatDetailResponse(
    Guid Id,
    string Status,
    string? Title,
    string? FacebookPostId,
    // The finalized draft copy/link (set once the chat is archived), so the client can preview the post text.
    string? PostMessage,
    string? PostLink,
    // True when a generated image exists; fetch it from GET /chats/{id}/image to preview before publishing.
    bool HasImage,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<MarketingMessageResponse> Messages);

public sealed record MarketingMessageResponse(
    Guid Id,
    string Role,
    string Source,
    string Content,
    string? DetectedIntent,
    DateTime CreatedAtUtc);
