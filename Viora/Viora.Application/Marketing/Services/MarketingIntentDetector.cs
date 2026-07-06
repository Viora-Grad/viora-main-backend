using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Application.Marketing.Abstractions;
using Viora.Domain.Marketing.Internal;
using Viora.Domain.Marketing.Prompts;

namespace Viora.Application.Marketing.Services;

// Groq-backed intent classifier. Mirrors AiRag's IntentDetectionService: stateless, temp 0.0, tolerant
// JSON parse. Only two outcomes; anything ambiguous/unparseable falls back to MarketingContent.
public sealed class MarketingIntentDetector(
    IChatCompletionService chat,
    ILogger<MarketingIntentDetector> logger) : IMarketingIntentDetector
{
    private static readonly PromptExecutionSettings Settings = new()
    {
        ExtensionData = new Dictionary<string, object>
        {
            ["temperature"] = 0.0,
            ["maxOutputTokens"] = 64,
        },
    };

    public async Task<MarketingIntent> DetectAsync(string message, CancellationToken cancellationToken)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(MarketingIntentPrompt.Build());
        history.AddUserMessage(message);

        try
        {
            var result = await chat.GetChatMessageContentAsync(history, Settings, cancellationToken: cancellationToken);
            return Parse(result.Content ?? string.Empty);
        }
        catch (Exception ex)
        {
            // Never let a classifier hiccup block the chat; default to content generation.
            logger.LogWarning(ex, "Marketing intent detection failed; defaulting to MarketingContent.");
            return MarketingIntent.MarketingContent;
        }
    }

    private static MarketingIntent Parse(string raw)
    {
        try
        {
            var json = raw.Replace("```json", "").Replace("```", "").Trim();
            using var doc = JsonDocument.Parse(json);
            var intentStr = doc.RootElement.TryGetProperty("intent", out var i) ? i.GetString() : null;

            return Enum.TryParse<MarketingIntent>(intentStr, true, out var intent)
                ? intent
                : MarketingIntent.MarketingContent;
        }
        catch
        {
            return MarketingIntent.MarketingContent;
        }
    }
}
