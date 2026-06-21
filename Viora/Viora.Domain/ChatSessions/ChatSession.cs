namespace Viora.Domain.ChatSessions;

// EF Core maps this to ChatSessions table.
public sealed class ChatSession
{
    public Guid Id { get; init; } = Guid.NewGuid();

    // User who owns this chat session
    public required Guid UserId { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Update on every flush from the semantic memory buffer, used for pruning old sessions
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    // Auto-generated title for the session,
    // extracted from the first user message by the LLM. Used for display in the UI.
    public string? Title { get; set;}

    // Full SK ChatHistory serialized with JsonSerializer.Serialize(chatHistory)
    // Overwritten on every flush — always the latest snapshot.
    public string HistoryJson { get; set; } = "[]";
}