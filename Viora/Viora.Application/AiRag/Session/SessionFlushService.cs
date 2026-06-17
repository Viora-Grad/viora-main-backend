// Called after EVERY turn. Serializes the current SK ChatHistory and writes
// it to the DB immediately — no waiting, no batching.
// First call for a session → CREATE row. Every subsequent call → UPDATE HistoryJson.
using System.Text.Json;
using Viora.Domain.ChatSessions;

namespace Viora.Application.AiRag.Session;

public sealed class SessionFlushService
{
    private readonly ChatSessionService _sessionService;
    private readonly IChatSessionRepository _repository;
    private readonly HashSet<Guid> _created = new();

    public SessionFlushService(ChatSessionService sessionService, IChatSessionRepository repository)
    {
        _sessionService = sessionService;
        _repository = repository;
    }

    public async Task FlushAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var json = _sessionService.SerializeHistory(sessionId.ToString());
        if (json == "[]") return;

        var now = DateTime.UtcNow;

        if (_created.Contains(sessionId))
        {
            await _repository.UpdateHistoryAsync(sessionId, json, now, ct);
        }
        else
        {
            var existing = await _repository.GetByIdAsync(sessionId, ct);
            if (existing is not null)
            {
                _created.Add(sessionId);
                await _repository.UpdateHistoryAsync(sessionId, json, now, ct);
            }
            else
            {
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
            using var doc = JsonDocument.Parse(json);
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (el.GetProperty("role").GetString() == "user")
                {
                    var text = el.GetProperty("content").GetString() ?? string.Empty;
                    return text.Length <= 80 ? text : text[..77] + "...";
                }
            }
        }
        catch { /* ignore */ }
        return null;
    }
}