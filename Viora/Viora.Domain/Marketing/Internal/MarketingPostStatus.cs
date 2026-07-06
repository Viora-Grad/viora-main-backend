namespace Viora.Domain.Marketing.Internal;

public enum MarketingPostStatus
{
    // The chat is open and no post has been created on Facebook yet.
    Draft,

    // The post was created on the Facebook Page as unpublished (archived). Quota was consumed here.
    Archived,

    // The archived post was flipped live on Facebook.
    Published,

    // A create attempt failed terminally (kept for auditing; the session can be retried while Draft).
    Failed
}
