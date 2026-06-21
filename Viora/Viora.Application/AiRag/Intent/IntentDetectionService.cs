using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Domain.AiRag.Intent;
using Viora.Domain.AiRag.Prompts;

namespace Viora.Application.AiRag.Intent;

public class IntentDetectionService
{
    private readonly IChatCompletionService _chat;

    public IntentDetectionService(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<DetectedIntent> DetectAsync(string message)
    {
        // Stateless — new ChatHistory per call, no conversation context
        var history = new ChatHistory();
        history.AddSystemMessage(IntentDetectionPrompt.Build());
        history.AddUserMessage(message);

        var settings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = 0.0, // Zero temp for deterministic classification
                ["maxOutputTokens"] = 256,
            },
        };

        var result = await _chat.GetChatMessageContentAsync(history, settings);
        return ParseResult(result.Content ?? string.Empty);
    }

    private static DetectedIntent ParseResult(string raw)
    {
        try
        {
            var json = raw.Replace("```json", "").Replace("```", "").Trim();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var intentStr = root.GetProperty("intent").GetString() ?? "Unclear";
            var confidence = root.TryGetProperty("confidence", out var c)
                ? c.GetString() ?? IntentConfidence.High
                : IntentConfidence.High;
            var query = root.TryGetProperty("extractedQuery", out var q) ? q.GetString() : null;
            var symptoms = root.TryGetProperty("extractedSymptoms", out var s) ? s.GetString() : null;

            if (!Enum.TryParse<ChatIntent>(intentStr, true, out var intent))
                intent = ChatIntent.Unclear;

            return new DetectedIntent
            {
                Intent = intent,
                Confidence = confidence,
                ExtractedQuery = query,
                ExtractedSymptoms = symptoms,
            };
        }
        catch
        {
            return new DetectedIntent
            {
                Intent = ChatIntent.Unclear, Confidence = IntentConfidence.Low
            };
        }
    }
}