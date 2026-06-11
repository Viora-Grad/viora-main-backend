using Viora.Domain.AiRag.Intent;

namespace Viora.Domain.AiRag.Chat;

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public List<ChatAction> Actions { get; set; } = new();
    public ChatIntent Intent { get; set; }
}