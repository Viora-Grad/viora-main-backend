using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Application.AiRag.Abstractions;
using Viora.Domain.AiRag;
using Viora.Domain.AiRag.Chat;
using Viora.Domain.AiRag.Intent;
using Viora.Domain.AiRag.Prompts;

namespace Viora.Application.AiRag.Handlers;

public sealed class SpecialtyHandler : IIntentHandler
{
    public ChatIntent Handles => ChatIntent.SpecialtyRecommendation;

    private readonly ISpecialtyVectorStore _store;
    private readonly IChatCompletionService _chat;
    private readonly Kernel _kernel;

    public SpecialtyHandler(ISpecialtyVectorStore store, IChatCompletionService chat, Kernel kernel)
    {
        _store = store;
        _chat = chat;
        _kernel = kernel;
    }

    public async Task<ChatResponse> HandleAsync(string message, DetectedIntent detected, ChatHistory history, UserContext? userContext = null)
    {
        var specialties = await _store.SearchAsync(message, topK: 5);

        if (specialties.Count == 0)
        {
            return new ChatResponse
            {
                Message = "I wasn't able to determine a specialty for your concern. Please describe your symptoms in more detail.",
                Intent = ChatIntent.SpecialtyRecommendation,
            };
        }

        var ragHistory = new ChatHistory();
        ragHistory.AddSystemMessage(SpecialtyRagPrompt.Build(specialties, userContext));
        ragHistory.AddUserMessage(message);

        var result = await _chat.GetChatMessageContentAsync(ragHistory, kernel: _kernel);
        var responseText = result.Content?.Trim() ?? "I'm sorry, I couldn't generate a response.";

        var knownSpecialties = specialties
            .Select(s => s.Specialty)
            .Distinct()
            .ToList();

        var mentionedSpecialties = ParseRecommendedSpecialties(responseText, knownSpecialties);

        var cleanMessage = RemoveSpecialtyMarker(responseText);
        var actions = mentionedSpecialties
            .Select(spec => new ChatAction
            {
                Label = $"Search for {spec}",
                Specialty = spec,
            })
            .ToList();

        return new ChatResponse
        {
            Message = cleanMessage,
            Intent = ChatIntent.SpecialtyRecommendation,
            Actions = actions,
        };
    }

    private static List<string> ParseRecommendedSpecialties(string text, List<string> knownSpecialties)
    {
        var markerIndex = text.IndexOf("[RECOMMENDED_SPECIALTIES:", StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return knownSpecialties
                .Where(spec => text.Contains(spec, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var afterMarker = text[(markerIndex + "[RECOMMENDED_SPECIALTIES:".Length)..];
        var endBracket = afterMarker.IndexOf(']');
        if (endBracket < 0) return [];

        var specialtiesStr = afterMarker[..endBracket];
        var parsed = specialtiesStr
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();

        var matched = new List<string>();
        foreach (var spec in parsed)
        {
            var match = knownSpecialties.FirstOrDefault(k =>
                k.Equals(spec, StringComparison.OrdinalIgnoreCase));
            if (match != null) matched.Add(match);
        }

        return matched;
    }

    private static string RemoveSpecialtyMarker(string text)
    {
        var markerIndex = text.IndexOf("[RECOMMENDED_SPECIALTIES:", StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0) return text;

        var endBracket = text.IndexOf(']', markerIndex);
        if (endBracket < 0) return text;

        var before = text[..markerIndex].TrimEnd();
        var after = text[(endBracket + 1)..].TrimStart();
        var result = before + (after.Length > 0 ? "\n" + after : "");
        return result.Trim();
    }
}