namespace Viora.Application.Marketing.GetDraftContent;

// The drafted post copy, decoded from the Manus attachment. ContentType reflects the attachment's media
// type (e.g. text/markdown) so the client can render it appropriately.
public sealed record MarketingDraftContentResponse(Guid ChatId, string Content, string ContentType);
