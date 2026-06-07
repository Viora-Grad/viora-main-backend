using Viora.Domain.Abstractions;

namespace Viora.Domain.Vivi.Messages;

public static class MessageErrors
{
    public static Error AgentFailedToLoadContent => new("Vivi.AgentFailedToLoadContent", "The agent returned empty content", ErrorCategory.BadGateway);
}
