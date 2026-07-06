using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.GetChat;

public sealed record GetChatQuery(Guid ChatId) : IQuery<MarketingChatDetailResponse>;
