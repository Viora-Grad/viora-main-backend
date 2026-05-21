using Viora.Domain.Abstractions;

namespace Viora.Domain.Vivi.ChatSessions;

public static class ChatSessionErrors
{
    public static Error ActivityTimeChatConflict => new("Vivi.ActivityTimeChatConflict", "The activity timeline conflicted with the current time track", ErrorCategory.Conflict);
}
