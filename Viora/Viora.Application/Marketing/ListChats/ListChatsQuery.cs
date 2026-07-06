using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.ListChats;

public sealed record ListChatsQuery : IQuery<IReadOnlyList<MarketingChatSummaryResponse>>;
