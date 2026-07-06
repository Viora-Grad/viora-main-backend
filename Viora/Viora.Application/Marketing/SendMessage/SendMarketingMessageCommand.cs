using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.SendMessage;

// Sends a message into a marketing chat. The handler is the orchestrator: it detects intent and routes to
// Manus (content) or dispatches the quota-consuming FinalizePostCommand (finalize). Not a limited-feature
// command itself — only the nested finalize step touches quota.
public sealed record SendMarketingMessageCommand(Guid ChatId, string Message) : ICommand<MarketingChatResponse>;
