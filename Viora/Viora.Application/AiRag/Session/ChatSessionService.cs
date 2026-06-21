using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Viora.Application.AiRag.Session;

public class ChatSessionService
{
    private readonly ILogger<ChatSessionService> _logger;
    // In-memory; swap for IDistributedCache in production
    private readonly ConcurrentDictionary<string, ChatHistory> _sessions = new();

    public ChatSessionService(ILogger<ChatSessionService> logger)
    {
        _logger = logger;
    }

    public ChatHistory GetOrCreate(string sessionId) =>
        _sessions.GetOrAdd(sessionId, _ => new ChatHistory());

    public void AppendUser(string sessionId, string message) =>
        GetOrCreate(sessionId).AddUserMessage(message);

    public void AppendAssistant(string sessionId, string message) =>
        GetOrCreate(sessionId).AddAssistantMessage(message);

    public string SerializeHistory(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var history))
        {
            var result = JsonSerializer.Serialize(history);
            _logger.LogInformation("SerializeHistory: sessionId={SessionId}, messageCount={Count}, json starts with={Start}",
                sessionId, history.Count, result.Length > 80 ? result[..80] : result);
            return result;
        }
        _logger.LogWarning("SerializeHistory: sessionId={SessionId} NOT FOUND in memory, returning []", sessionId);
        return "[]";
    }

    public bool IsLoaded(string sessionId) =>
        _sessions.ContainsKey(sessionId);

    public void DeserializeInto(string sessionId, string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            _logger.LogWarning("DeserializeInto: sessionId={SessionId} has no history (json empty or '[]')", sessionId);
            return;
        }
        try
        {
            var history = JsonSerializer.Deserialize<ChatHistory>(json);
            if (history is not null)
            {
                _sessions[sessionId] = history;
                _logger.LogInformation("DeserializeInto: sessionId={SessionId} loaded with {Count} messages", sessionId, history.Count);
            }
            else
            {
                _logger.LogWarning("DeserializeInto: sessionId={SessionId} deserialized to null", sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeserializeInto: sessionId={SessionId} FAILED to deserialize", sessionId);
        }
    }
}