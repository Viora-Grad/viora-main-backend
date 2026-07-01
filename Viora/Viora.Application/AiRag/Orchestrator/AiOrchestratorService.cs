using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Application.AiRag.Abstractions;
using Viora.Application.AiRag.Handlers;
using Viora.Application.AiRag.Intent;
using Viora.Application.AiRag.Session;
using Viora.Domain.AiRag;
using Viora.Domain.AiRag.Chat;
using Viora.Domain.AiRag.Intent;

namespace Viora.Application.AiRag.Orchestrator;

public class AiOrchestratorService
{
    private readonly IntentDetectionService _intentDetection;
    private readonly IEnumerable<IIntentHandler> _handlers;
    private readonly ChatSessionService _sessions;
    private readonly SessionFlushService _flush;
    private readonly LoadSessionCommand _loadSession;
    private readonly IUserProfileService _userProfile;

    public AiOrchestratorService(
        IntentDetectionService intentDetection,
        IEnumerable<IIntentHandler> handlers,
        ChatSessionService sessions,
        SessionFlushService flush,
        LoadSessionCommand loadSession,
        IUserProfileService userProfile)
    {
        _intentDetection = intentDetection;
        _handlers = handlers;
        _sessions = sessions;
        _flush = flush;
        _loadSession = loadSession;
        _userProfile = userProfile;
    }

    public async Task<ChatResponse> HandleAsync(ChatRequest request, Guid userId, CancellationToken ct = default)
    {
        var sessionId = request.SessionId ?? Guid.NewGuid().ToString();

        // Step 0 — Load existing session from DB if not already in memory
        if (request.SessionId is not null)
            await _loadSession.ExecuteAsync(Guid.Parse(sessionId), userId, ct);

        // Step 0.5 — Fetch user context (name + medical record) for personalization
        var userContext = await _userProfile.GetUserContextAsync(userId, ct);

        // Step 0.75 — Inject user context into session history for handlers that copy from history
        var history = _sessions.GetOrCreate(sessionId);
        if (history.Count == 0 && userContext is not null)
        {
            var ctxMsg = BuildUserContextMessage(userContext);
            history.AddSystemMessage(ctxMsg);
        }

        // Step 1 — Classify the message (stateless, no history needed)
        var detected = await _intentDetection.DetectAsync(request.Message);

        // Step 2 — Route to the correct handler
        var handler = _handlers.FirstOrDefault(h => h.Handles == detected.Intent)
                      ?? _handlers.First(h => h.Handles == ChatIntent.General);

        // Step 3 — Handler receives the full session ChatHistory for context
        var response = await handler.HandleAsync(request.Message, detected, history, userContext);

        // Step 4 — Append user + assistant to session history (after handler)
        _sessions.AppendUser(sessionId, request.Message);
        _sessions.AppendAssistant(sessionId, response.Message);

        // Step 5 — Flush session to database
        await _flush.FlushAsync(Guid.Parse(sessionId), userId, ct);

        response.Intent = detected.Intent;
        response.SessionId = sessionId;
        return response;
    }

    private static string BuildUserContextMessage(UserContext userContext)
    {
        var sb = $"The user's name is {userContext.FirstName}. Always address them by their name.";
        if (userContext.MedicalRecordSummary is not null)
            sb += $"\nUSER'S MEDICAL RECORD: {userContext.MedicalRecordSummary}\nActively reference this medical information in your responses. For example, mention their specific conditions, allergies, or vitals when relevant, and explain how it affects your recommendations. Be interactive - say things like \"Because you have X, I recommend Y instead of Z.\"";

        return $"USER CONTEXT (use this to personalize your response):\n{sb}";
    }
}