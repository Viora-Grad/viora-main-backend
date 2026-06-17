using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Viora.Application.AiRag.Session;

public class ChatSessionService
{
    // In-memory; swap for IDistributedCache in production
    private readonly ConcurrentDictionary<string, ChatHistory> _sessions = new();

    public ChatHistory GetOrCreate(string sessionId) =>
        _sessions.GetOrAdd(sessionId, _ => new ChatHistory());

    public void AppendUser(string sessionId, string message) =>
        GetOrCreate(sessionId).AddUserMessage(message);

    public void AppendAssistant(string sessionId, string message) =>
        GetOrCreate(sessionId).AddAssistantMessage(message);

    public string SerializeHistory(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var history))
            return JsonSerializer.Serialize(history);
        return "[]";
    }

    public bool IsLoaded(string sessionId) =>
        _sessions.ContainsKey(sessionId);

    public void DeserializeInto(string sessionId, string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]") return;
        var history = JsonSerializer.Deserialize<ChatHistory>(json);
        if (history is not null)
            _sessions[sessionId] = history;
    }
}