using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Domain.ChatSessions;

namespace Viora.Application.AiRag.Session;

public sealed record SessionHistoryDto(Guid SessionId, string? Title, DateTime CreatedAt, IReadOnlyList<SessionMessageDto> Messages);
public sealed record SessionMessageDto(string Role, string Content, int Index);
public sealed record SessionSummaryDto(Guid SessionId, string? Title, DateTime CreatedAt, DateTime LastActiveAt);

public sealed class GetSessionHistoryQuery
{
    private readonly IChatSessionRepository _repository;
    private readonly ILogger<GetSessionHistoryQuery> _logger;

    public GetSessionHistoryQuery(IChatSessionRepository repository, ILogger<GetSessionHistoryQuery> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<SessionHistoryDto?> ExecuteAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var session = await _repository.GetByIdAsync(sessionId, ct);
        if (session is null || session.UserId != userId)
        {
            _logger.LogWarning("ExecuteAsync: sessionId={SessionId} not found or wrong user", sessionId);
            return null;
        }

        _logger.LogInformation("ExecuteAsync: sessionId={SessionId}, HistoryJson length={Len}, starts with={Start}",
            sessionId, session.HistoryJson?.Length ?? 0, session.HistoryJson?.Length > 50 ? session.HistoryJson[..50] : session.HistoryJson);

        var messages = ParseJson(session.HistoryJson);
        _logger.LogInformation("ExecuteAsync: sessionId={SessionId}, parsed {Count} messages", sessionId, messages.Count);
        return new SessionHistoryDto(session.Id, session.Title, session.CreatedAt, messages);
    }

    private static IReadOnlyList<SessionMessageDto> ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]") return [];
        try
        {
            var history = JsonSerializer.Deserialize<ChatHistory>(json);
            if (history is null) return [];
            return history
                .Select((msg, i) => new SessionMessageDto(
                    Role: msg.Role.ToString(),
                    Content: msg.Content ?? string.Empty,
                    Index: i))
                .ToList();
        }
        catch { return []; }
    }
}