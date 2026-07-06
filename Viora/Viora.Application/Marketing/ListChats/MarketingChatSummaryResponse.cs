namespace Viora.Application.Marketing.ListChats;

public sealed record MarketingChatSummaryResponse(
    Guid Id,
    string? Title,
    string Status,
    string? FacebookPostId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
