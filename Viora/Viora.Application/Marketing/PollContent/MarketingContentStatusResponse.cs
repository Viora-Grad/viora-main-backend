namespace Viora.Application.Marketing.PollContent;

// Status: "Pending" (Manus still working), "Ready" (Content is the latest draft copy),
// "None" (no task in flight and no draft yet).
public sealed record MarketingContentStatusResponse(Guid ChatId, string Status, string? Content);
