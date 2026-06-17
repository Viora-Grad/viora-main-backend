using System.Text.Json.Serialization;

namespace Viora.Domain.AiRag.Intent;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatIntent
{
    Greeting,
    General,
    SpecialtyRecommendation,
    KnowledgeQuery,
    Unclear,
}