using System.Collections.Concurrent;
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
}