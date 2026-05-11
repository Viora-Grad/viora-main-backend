using Viora.Domain.Abstractions;

namespace Viora.Domain.Vivi;

public static class ViviErrors
{
    public static Error AgentFailedToLoadContent => new("Vivi.AgentFailedToLoadContent", "The agent returned empty content", ErrorCategory.BadGateway);
    public static Error ActivityTimeChatConflict => new("Vivi.ActivityTimeChatConflict", "The activity timeline conflicted with the current time track", ErrorCategory.Conflict);
}
