using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.GetDraftContent;

// Fetches the drafted post copy (proxied from the Manus attachment) so it can be previewed before publishing.
public sealed record GetDraftContentQuery(Guid ChatId) : IQuery<MarketingDraftContentResponse>;
