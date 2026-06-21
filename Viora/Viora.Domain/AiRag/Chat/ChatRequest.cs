using System.ComponentModel.DataAnnotations;

namespace Viora.Domain.AiRag.Chat;

public class ChatRequest
{
    [Required, MinLength(1), MaxLength(4000)]
    public string Message { get; set; } = string.Empty;
    
    public string? SessionId { get; set; }
}