using Viora.Application.AiRag.Handlers;
using Viora.Application.AiRag.Intent;
using Viora.Application.AiRag.Session;
using Viora.Domain.AiRag.Chat;
using Viora.Domain.AiRag.Intent;

namespace Viora.Application.AiRag.Orchestrator;

public class AiOrchestratorService
{
    private readonly IntentDetectionService _intentDetection;
    private readonly IEnumerable<IIntentHandler> _handlers;
    private readonly ChatSessionService _sessions;

    public AiOrchestratorService(
        IntentDetectionService intentDetection,
        IEnumerable<IIntentHandler> handlers,
        ChatSessionService sessions)
    {
        _intentDetection = intentDetection;
        _handlers = handlers;
        _sessions = sessions;
    }

    public async Task<ChatResponse> ProcessAsync(ChatRequest request)
    {
        var sessionId = request.SessionId ?? Guid.NewGuid().ToString();

        // Step 1 — Classify the message (stateless, no history needed)
        var detected = await _intentDetection.DetectAsync(request.Message);

        // Step 2 — Append user turn to session history
        _sessions.AppendUser(sessionId, request.Message);

        // Step 3 — Route to the correct handler
        var handler = _handlers.FirstOrDefault(h => h.Handles == detected.Intent)
                      ?? _handlers.First(h => h.Handles == ChatIntent.Unclear);

        // Step 4 — Handler receives the full session ChatHistory for context
        var history = _sessions.GetOrCreate(sessionId);
        var response = await handler.HandleAsync(request.Message, detected, history);

        // Step 5 — Append assistant reply to session history
        _sessions.AppendAssistant(sessionId, response.Message);

        response.Intent = detected.Intent;
        return response;
    }
}