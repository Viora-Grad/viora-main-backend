namespace Viora.Domain.Marketing.Internal;

// Which engine produced an assistant message (User for user-authored messages).
public enum MessageSource
{
    User,
    Groq,
    Manus
}
