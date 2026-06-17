using System.Text.Json;
using Viora.Domain.ChatSessions;

namespace Viora.Application.AiRag.Session;

public sealed record SessionHistoryDto(Guid SessionId, string? Title, DateTime CreatedAt, IReadOnlyList<SessionMessageDto> Messages);
public sealed record SessionMessageDto(string Role, string Content, int Index);
public sealed record SessionSummaryDto(Guid SessionId, string? Title, DateTime CreatedAt, DateTime LastActiveAt);

public sealed class GetSessionHistoryQuery
{
    private readonly IChatSessionRepository _repository;

    public GetSessionHistoryQuery(IChatSessionRepository repository) => _repository = repository;

    public async Task<SessionHistoryDto?> ExecuteAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var session = await _repository.GetByIdAsync(sessionId, ct);
        if (session is null || session.UserId != userId) return null;

        var messages = ParseJson(session.HistoryJson);
        return new SessionHistoryDto(session.Id, session.Title, session.CreatedAt, messages);
    }

    private static IReadOnlyList<SessionMessageDto> ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]") return [];
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.EnumerateArray()
                .Select((el, i) => new SessionMessageDto(
                    Role: el.GetProperty("role").GetString() ?? "unknown",
                    Content: el.GetProperty("content").GetString() ?? string.Empty,
                    Index: i))
                .ToList();
        }
        catch { return []; }
    }
}