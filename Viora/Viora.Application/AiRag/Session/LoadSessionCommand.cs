using Viora.Domain.ChatSessions;

namespace Viora.Application.AiRag.Session;

public sealed class LoadSessionCommand
{
    private readonly IChatSessionRepository _repository;
    private readonly ChatSessionService _sessionService;

    public LoadSessionCommand(IChatSessionRepository repository, ChatSessionService sessionService)
    {
        _repository = repository;
        _sessionService = sessionService;
    }

    /// <summary>
    /// Loads a session from DB into in-memory SK ChatHistory.
    /// Returns false if not found or belongs to a different user.
    /// No-op if already loaded in memory.
    /// </summary>
    public async Task<bool> ExecuteAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var sid = sessionId.ToString();
        if (_sessionService.IsLoaded(sid)) return true;

        var session = await _repository.GetByIdAsync(sessionId, ct);
        if (session is null || session.UserId != userId) return false;

        _sessionService.DeserializeInto(sid, session.HistoryJson);
        return true;
    }
}