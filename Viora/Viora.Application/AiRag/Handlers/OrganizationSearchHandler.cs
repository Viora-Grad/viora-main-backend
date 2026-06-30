using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Domain.AiRag.Chat;
using Viora.Domain.AiRag.Intent;

namespace Viora.Application.AiRag.Handlers;

public sealed class OrganizationSearchHandler : IIntentHandler
{
    public ChatIntent Handles => ChatIntent.OrganizationSearch;

    private readonly IChatCompletionService _chat;
    private readonly Kernel _kernel;

    private static readonly PromptExecutionSettings JsonSettings = new()
    {
        ExtensionData = new Dictionary<string, object>
        {
            ["temperature"] = 0.0,
            ["max_tokens"] = 512,
        },
    };

    private const string ExtractionPrompt = """
You are a search intent parser for Viora, a healthcare platform.
Given the user's message, extract organization search filters.

Only extract these supported filters:
- orgName: organization name (e.g. "Viora Health", "Sunrise Clinic")
- country: country name (e.g. "Egypt", "USA")
- serviceType: medical service type (e.g. "Cardiology", "Dermatology", "Orthopedic Surgery")
- minRating: minimum rating out of 10 (number, 0-10)

If the user mentions a filter that is NOT supported (e.g. "price", "insurance", "distance", "language"),
include it in "unsupportedFilters" array.

Return ONLY valid JSON. No markdown. No explanation.

{
  "orgName": "string or null",
  "country": "string or null",
  "serviceType": "string or null",
  "minRating": number or null,
  "unsupportedFilters": ["string"],
  "responseMessage": "friendly confirmation of what was understood, or if unsupported filters exist, explain which filters are not supported and offer the supported ones"
}

Examples:
User: "find me a cardiology clinic in Egypt"
{"orgName":null,"country":"Egypt","serviceType":"Cardiology","minRating":null,"unsupportedFilters":[],"responseMessage":"I'll search for cardiology providers in Egypt for you."}

User: "show me top-rated hospitals in USA with cardiology"
{"orgName":null,"country":"USA","serviceType":"Cardiology","minRating":null,"unsupportedFilters":[],"responseMessage":"I'll search for top cardiology providers in the USA."}

User: "find Viora Health"
{"orgName":"Viora Health","country":null,"serviceType":null,"minRating":null,"unsupportedFilters":[],"responseMessage":"I'll search for Viora Health."}

User: "find a cheap dentist near me"
{"orgName":null,"country":null,"serviceType":"Dentist","minRating":null,"unsupportedFilters":["cheap","near me"],"responseMessage":"I can't search by price or distance, but I can search for dentists by name, country, or rating."}
""";

    public OrganizationSearchHandler(IChatCompletionService chat, Kernel kernel)
    {
        _chat = chat;
        _kernel = kernel;
    }

    public async Task<ChatResponse> HandleAsync(string message, DetectedIntent detected, ChatHistory history)
    {
        var extractionHistory = new ChatHistory();
        extractionHistory.AddSystemMessage(ExtractionPrompt);
        extractionHistory.AddUserMessage(message);

        var result = await _chat.GetChatMessageContentAsync(extractionHistory, JsonSettings, _kernel);
        var json = result.Content?.Trim() ?? "";

        var parsed = ParseExtraction(json);

        if (parsed == null)
        {
            return new ChatResponse
            {
                Message = "I'm sorry, I couldn't understand your search request. You can search for organizations by name, country, service type, or minimum rating.",
                Intent = ChatIntent.OrganizationSearch,
            };
        }

        var hasAnyFilter = parsed.OrgName != null
            || parsed.Country != null
            || parsed.ServiceType != null
            || parsed.MinRating != null;

        var actions = new List<ChatAction>();

        if (parsed.UnsupportedFilters.Count > 0)
        {
            var unsupportedList = string.Join(", ", parsed.UnsupportedFilters);
            var supported = new List<string>();
            if (parsed.OrgName != null) supported.Add($"name: \"{parsed.OrgName}\"");
            if (parsed.Country != null) supported.Add($"country: {parsed.Country}");
            if (parsed.ServiceType != null) supported.Add($"service type: {parsed.ServiceType}");
            if (parsed.MinRating != null) supported.Add($"minimum rating: {parsed.MinRating}/10");

            if (supported.Count > 0)
            {
                var responseMsg = $"I can't search using filters like {unsupportedList}, but I can search using what you mentioned: {string.Join(", ", supported)}. Would you like me to search with these filters?";

                actions.Add(new ChatAction
                {
                    Label = $"Search with available filters",
                    ActionType = "orgSearch",
                    OrgName = parsed.OrgName,
                    Country = parsed.Country,
                    ServiceType = parsed.ServiceType,
                    MinRating = parsed.MinRating,
                });

                return new ChatResponse
                {
                    Message = responseMsg,
                    Intent = ChatIntent.OrganizationSearch,
                    Actions = actions,
                };
            }

            return new ChatResponse
            {
                Message = $"I can't search using filters like {unsupportedList}. I can only search by organization name, country, service type, or minimum rating. Could you try one of those?",
                Intent = ChatIntent.OrganizationSearch,
            };
        }

        if (!hasAnyFilter)
        {
            return new ChatResponse
            {
                Message = "I can help you search for healthcare organizations! You can search by name, country, service type (like Cardiology, Dermatology), or minimum rating. What would you like to search for?",
                Intent = ChatIntent.OrganizationSearch,
            };
        }

        var label = "Search";
        var parts = new List<string>();
        if (parsed.OrgName != null) parts.Add($"\"{parsed.OrgName}\"");
        if (parsed.Country != null) parts.Add($"in {parsed.Country}");
        if (parsed.ServiceType != null) parts.Add(parsed.ServiceType);

        label = $"Search {string.Join(" ", parts)}";

        if (parsed.MinRating != null)
        {
            label += $" (min {parsed.MinRating}/10)";
        }

        actions.Add(new ChatAction
        {
            Label = label,
            ActionType = "orgSearch",
            OrgName = parsed.OrgName,
            Country = parsed.Country,
            ServiceType = parsed.ServiceType,
            MinRating = parsed.MinRating,
        });

        return new ChatResponse
        {
            Message = parsed.ResponseMessage ?? $"Sure! I'll search for healthcare organizations{("" + (parsed.OrgName != null ? $" matching \"{parsed.OrgName}\"" : "") + (parsed.Country != null ? $" in {parsed.Country}" : "") + (parsed.ServiceType != null ? $" offering {parsed.ServiceType}" : "") + (parsed.MinRating != null ? $" with minimum rating {parsed.MinRating}/10" : ""))}. Tap the button below to see results!",
            Intent = ChatIntent.OrganizationSearch,
            Actions = actions,
        };
    }

    private static ParsedOrgSearch? ParseExtraction(string raw)
    {
        try
        {
            var json = raw.Replace("```json", "").Replace("```", "").Trim();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new ParsedOrgSearch
            {
                OrgName = root.TryGetProperty("orgName", out var n) ? n.GetString() : null,
                Country = root.TryGetProperty("country", out var c) ? c.GetString() : null,
                ServiceType = root.TryGetProperty("serviceType", out var s) ? s.GetString() : null,
                MinRating = root.TryGetProperty("minRating", out var r) && r.ValueKind == JsonValueKind.Number ? r.GetDouble() : null,
                UnsupportedFilters = root.TryGetProperty("unsupportedFilters", out var u) && u.ValueKind == JsonValueKind.Array
                    ? u.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToList()
                    : [],
                ResponseMessage = root.TryGetProperty("responseMessage", out var m) ? m.GetString() : null,
            };
        }
        catch
        {
            return null;
        }
    }

    private sealed class ParsedOrgSearch
    {
        public string? OrgName { get; set; }
        public string? Country { get; set; }
        public string? ServiceType { get; set; }
        public double? MinRating { get; set; }
        public List<string> UnsupportedFilters { get; set; } = [];
        public string? ResponseMessage { get; set; }
    }
}