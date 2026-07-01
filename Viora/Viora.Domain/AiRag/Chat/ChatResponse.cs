using Viora.Domain.AiRag.Intent;

namespace Viora.Domain.AiRag.Chat;

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public ChatIntent Intent { get; set; }
    public List<ChatAction> Actions { get; set; } = [];
}