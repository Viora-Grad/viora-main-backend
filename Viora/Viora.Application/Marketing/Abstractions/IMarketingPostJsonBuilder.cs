using Viora.Domain.Abstractions;

namespace Viora.Application.Marketing.Abstractions;

// Groq-backed builder: turns the latest Manus idea + conversation into the final Facebook post content.
public interface IMarketingPostJsonBuilder
{
    Task<Result<GeneratedPost>> BuildAsync(
        string manusIdea,
        string conversationContext,
        CancellationToken cancellationToken);
}

// The model's structured output: a short title + the post message + an optional link.
public sealed record GeneratedPost(string? Title, string Message, string? Link);
