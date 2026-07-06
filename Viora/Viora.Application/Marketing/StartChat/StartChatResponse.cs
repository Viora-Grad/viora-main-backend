using Viora.Application.Marketing.SendMessage;

namespace Viora.Application.Marketing.StartChat;

// FirstReply is populated only when a first message was provided and processed successfully.
public sealed record StartChatResponse(Guid ChatId, MarketingChatResponse? FirstReply);
