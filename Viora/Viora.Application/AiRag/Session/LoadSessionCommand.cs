using Microsoft.Extensions.Logging;
using Viora.Domain.ChatSessions;

namespace Viora.Application.AiRag.Session;

public sealed class LoadSessionCommand
{
    private readonly IChatSessionRepository _repository;
    private readonly ChatSessionService _sessionService;
    private readonly ILogger<LoadSessionCommand> _logger;

    public LoadSessionCommand(IChatSessionRepository repository, ChatSessionService sessionService, ILogger<LoadSessionCommand> logger)
    {
        _repository = repository;
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Loads a session from DB into in-memory SK ChatHistory.
    /// Returns false if not found or belongs to a different user.
    /// No-op if already loaded in memory.
    /// </summary>
    public async Task<bool> ExecuteAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var sid = sessionId.ToString();
        if (_sessionService.IsLoaded(sid))
        {
            _logger.LogInformation("LoadSessionCommand: sessionId={SessionId} already loaded, skipping", sessionId);
            return true;
        }

        var session = await _repository.GetByIdAsync(sessionId, ct);
        if (session is null)
        {
            _logger.LogWarning("LoadSessionCommand: sessionId={SessionId} not found in DB", sessionId);
            return false;
        }
        if (session.UserId != userId)
        {
            _logger.LogWarning("LoadSessionCommand: sessionId={SessionId} belongs to different user", sessionId);
            return false;
        }

        _logger.LogInformation("LoadSessionCommand: sessionId={SessionId} found, HistoryJson length={Len}", sessionId, session.HistoryJson?.Length ?? 0);
        _sessionService.DeserializeInto(sid, session.HistoryJson);
        return true;
    }
}