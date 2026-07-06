using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.GetDraftImage;

// Fetches the generated draft image bytes (proxied from Manus) so it can be previewed before publishing.
public sealed record GetDraftImageQuery(Guid ChatId) : IQuery<MarketingImageResponse>;
