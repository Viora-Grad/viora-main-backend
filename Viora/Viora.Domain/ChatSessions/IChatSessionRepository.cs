namespace Viora.Domain.ChatSessions;

// Persistance contract for AI chat sessions.
// Implmented in Infrastructure layer with EF Core
// Consumed by Application (SessionFlushService, LoadSessionCommand, GetSessionHistoryQuery).
public interface IChatSessionRepository
{
    // Create a new chat session
    Task CreateAsync(ChatSession session, CancellationToken ct = default);

    // Update the history of an existing chat session
    Task UpdateHistoryAsync(Guid sessionId, string historyJson, DateTime lastActiveAt, CancellationToken ct = default);

    // Get a chat session by its ID, including all messages
    Task<ChatSession?> GetByIdAsync(Guid sessionId, CancellationToken ct = default);

    // Get paginated chat sessions for a user, ordered by last active time desc
    Task<IReadOnlyList<ChatSession>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}