using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Application.Marketing.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;
using Viora.Domain.Marketing.Prompts;

namespace Viora.Application.Marketing.Services;

// Groq-backed builder. Embeds the Meta /feed content schema in the system prompt and parses the model's
// JSON. Retries once with a correction nudge if the first attempt is malformed. The model returns CONTENT
// FIELDS ONLY (title/message/link) — token and page id are injected by the finalize handler, never here.
public sealed class MarketingPostJsonBuilder(
    IChatCompletionService chat,
    ILogger<MarketingPostJsonBuilder> logger) : IMarketingPostJsonBuilder
{
    private static readonly PromptExecutionSettings Settings = new()
    {
        ExtensionData = new Dictionary<string, object>
        {
            ["temperature"] = 0.2,
            ["max_tokens"] = 1024,
        },
    };

    public async Task<Result<GeneratedPost>> BuildAsync(string manusIdea, string conversationContext, CancellationToken cancellationToken)
    {
        var userContent =
            $"Marketing idea to turn into a post:\n{manusIdea}\n\nConversation so far:\n{conversationContext}";

        var post = await AttemptAsync(userContent, correction: null, cancellationToken)
                   ?? await AttemptAsync(userContent,
                        correction: "Your previous reply was not valid JSON matching the required shape. " +
                                    "Reply again with ONLY the JSON object, no markdown, no extra text.",
                        cancellationToken);

        if (post is null)
        {
            logger.LogWarning("Marketing post JSON builder produced no valid payload after retry.");
            return Result.Failure<GeneratedPost>(MarketingErrors.ContentGenerationFailed);
        }

        if (string.IsNullOrWhiteSpace(post.Message))
            return Result.Failure<GeneratedPost>(MarketingErrors.PayloadInvalid);

        return Result.Success(post);
    }

    private async Task<GeneratedPost?> AttemptAsync(string userContent, string? correction, CancellationToken cancellationToken)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(MetaPostJsonPrompt.Build());
        history.AddUserMessage(userContent);
        if (correction is not null)
            history.AddSystemMessage(correction);

        try
        {
            var result = await chat.GetChatMessageContentAsync(history, Settings, cancellationToken: cancellationToken);
            return Parse(result.Content ?? string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Marketing post JSON builder LLM call failed.");
            return null;
        }
    }

    private static GeneratedPost? Parse(string raw)
    {
        try
        {
            var json = raw.Replace("```json", "").Replace("```", "").Trim();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var message = root.TryGetProperty("message", out var m) ? m.GetString() : null;
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var title = root.TryGetProperty("title", out var t) ? t.GetString() : null;
            var link = root.TryGetProperty("link", out var l) && l.ValueKind == JsonValueKind.String
                ? l.GetString()
                : null;

            return new GeneratedPost(title, message!, NormalizeLink(link));
        }
        catch
        {
            return null;
        }
    }

    // Keeps only a valid absolute http(s) URL; drops placeholders / non-URLs the model might emit.
    private static string? NormalizeLink(string? link)
    {
        if (string.IsNullOrWhiteSpace(link))
            return null;

        return Uri.TryCreate(link, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? link
            : null;
    }
}
