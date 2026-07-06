using Viora.Domain.Marketing.Internal;

namespace Viora.Application.Marketing.Abstractions;

// Groq-backed classifier. Returns MarketingContent | FinalizePost (fallback MarketingContent).
public interface IMarketingIntentDetector
{
    Task<MarketingIntent> DetectAsync(string message, CancellationToken cancellationToken);
}
