// Called after EVERY turn. Serializes the current SK ChatHistory and writes
// it to the DB immediately — no waiting, no batching.
// First call for a session → CREATE row. Every subsequent call → UPDATE HistoryJson.
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Domain.ChatSessions;

namespace Viora.Application.AiRag.Session;

public sealed class SessionFlushService
{
    private readonly ChatSessionService _sessionService;
    private readonly IChatSessionRepository _repository;
    private readonly ILogger<SessionFlushService> _logger;
    private readonly HashSet<Guid> _created = new();

    public SessionFlushService(ChatSessionService sessionService, IChatSessionRepository repository, ILogger<SessionFlushService> logger)
    {
        _sessionService = sessionService;
        _repository = repository;
        _logger = logger;
    }

    public async Task FlushAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var json = _sessionService.SerializeHistory(sessionId.ToString());
        _logger.LogInformation("FlushAsync: sessionId={SessionId}, json.Length={Len}, json starts with={Start}, alreadyCreated={Created}",
            sessionId, json.Length, json.Length > 50 ? json[..50] : json, _created.Contains(sessionId));
        if (json == "[]")
        {
            _logger.LogWarning("FlushAsync: skipping save for sessionId={SessionId} because history is empty", sessionId);
            return;
        }

        var now = DateTime.UtcNow;

        if (_created.Contains(sessionId))
        {
            _logger.LogInformation("FlushAsync: updating KNOWN sessionId={SessionId}", sessionId);
            await _repository.UpdateHistoryAsync(sessionId, json, now, ct);
        }
        else
        {
            var existing = await _repository.GetByIdAsync(sessionId, ct);
            _logger.LogInformation("FlushAsync: existing={Existing} for sessionId={SessionId}", existing is not null, sessionId);
            if (existing is not null)
            {
                _created.Add(sessionId);
                await _repository.UpdateHistoryAsync(sessionId, json, now, ct);
            }
            else
            {
                _logger.LogInformation("FlushAsync: CREATING new session sessionId={SessionId}", sessionId);
                await _repository.CreateAsync(new ChatSession
                {
                    Id = sessionId,
                    UserId = userId,
                    CreatedAt = now,
                    LastActiveAt = now,
                    Title = ExtractTitle(json),
                    HistoryJson = json,
                }, ct);

                _created.Add(sessionId);
            }
        }
    }

    private static string? ExtractTitle(string json)
    {
        try
        {
            var history = JsonSerializer.Deserialize<ChatHistory>(json);
            if (history is null) return null;
            var firstUser = history.FirstOrDefault(m => m.Role == AuthorRole.User);
            if (firstUser is null) return null;
            var text = firstUser.Content ?? string.Empty;
            return text.Length <= 80 ? text : text[..77] + "...";
        }
        catch { /* ignore */ }
        return null;
    }
}