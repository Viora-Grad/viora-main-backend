namespace Viora.Domain.Marketing.Internal;

// Only two intents: there is no general-chat branch. A non-finalize message is always treated as a
// marketing-content request (routed to Manus); the intent detector falls back to MarketingContent.
public enum MarketingIntent
{
    MarketingContent,
    FinalizePost
}
