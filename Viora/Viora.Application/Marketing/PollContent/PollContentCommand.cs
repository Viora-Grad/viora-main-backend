using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.PollContent;

// Polls the in-flight Manus generation for a chat. When Manus reports the task done, the copy is stored as
// the chat's draft idea (+ an assistant message) and returned. Safe to call repeatedly.
public sealed record PollContentCommand(Guid ChatId) : ICommand<MarketingContentStatusResponse>;
