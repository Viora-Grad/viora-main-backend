using Viora.Domain.Abstractions;
using Viora.Domain.Marketing.Internal;

namespace Viora.Domain.Marketing;

// A single turn in a marketing chat session. Child of MarketingChatSession; not an aggregate root.
public sealed class MarketingChatMessage : Entity
{
    public Guid SessionId { get; private set; }
    public MessageRole Role { get; private set; }
    public MessageSource Source { get; private set; }
    public string Content { get; private set; } = default!;

    // Stored for auditing which intent the classifier assigned to a user turn (null for assistant turns).
    public MarketingIntent? DetectedIntent { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private MarketingChatMessage() { }

    public static MarketingChatMessage Create(
        Guid sessionId,
        MessageRole role,
        MessageSource source,
        string content,
        MarketingIntent? detectedIntent,
        DateTime currentDateTime)
    {
        return new MarketingChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = role,
            Source = source,
            Content = content,
            DetectedIntent = detectedIntent,
            CreatedAtUtc = currentDateTime
        };
    }
}
