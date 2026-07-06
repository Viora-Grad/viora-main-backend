using Viora.Domain.Abstractions;
using Viora.Domain.Marketing.Internal;

namespace Viora.Domain.Marketing;

// Aggregate root: one "post-in-progress" conversation for a Facebook Page post, owned by an organization.
public sealed class MarketingChatSession : Entity
{
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public string? Title { get; private set; }
    public MarketingPostStatus Status { get; private set; }

    // Snapshot of the last marketing copy Manus produced for this chat; the finalize step turns this into
    // the Meta post payload. Null until the first MarketingContent turn completes.
    public string? LatestManusIdea { get; private set; }

    // Manus URL of the latest generated image for this chat (if any). When set, publish uploads it as a
    // Facebook photo post; otherwise it creates a text/link post.
    public string? LatestImageUrl { get; private set; }

    // Manus URL of the latest post-copy attachment (if any). Manus delivers the full copy as an attached
    // document; this is proxied and decoded on demand so the user can preview the drafted content.
    public string? LatestContentUrl { get; private set; }

    // Manus runs content generation as an async task. While one is in flight these hold its id/url; the poll
    // step reads the result via Manus and, once ready, moves the text into LatestManusIdea and clears these.
    public string? PendingManusTaskId { get; private set; }
    public string? PendingManusTaskUrl { get; private set; }

    // The finalized draft content, stored locally when the chat is archived. Nothing is created on Facebook
    // until publish, which turns these (+ LatestImageUrl) into the live post.
    public string? PostMessage { get; private set; }
    public string? PostLink { get; private set; }

    // The live post id returned by Facebook at publish time. Null while the draft is only local.
    public string? FacebookPostId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private readonly List<MarketingChatMessage> _messages = [];
    public IReadOnlyCollection<MarketingChatMessage> Messages => _messages.AsReadOnly();

    private MarketingChatSession() { }

    public static MarketingChatSession Create(Guid organizationId, Guid userId, DateTime currentDateTime)
    {
        return new MarketingChatSession
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            UserId = userId,
            Status = MarketingPostStatus.Draft,
            CreatedAtUtc = currentDateTime,
            UpdatedAtUtc = currentDateTime
        };
    }

    public MarketingChatMessage AddMessage(
        MessageRole role,
        MessageSource source,
        string content,
        MarketingIntent? detectedIntent,
        DateTime currentDateTime)
    {
        var message = MarketingChatMessage.Create(Id, role, source, content, detectedIntent, currentDateTime);
        _messages.Add(message);
        UpdatedAtUtc = currentDateTime;
        return message;
    }

    // Stores the generated copy and (optionally) the generated image URL and post-copy attachment URL from
    // a completed Manus task.
    public void SetManusIdea(string idea, string? imageUrl, string? contentUrl, DateTime currentDateTime)
    {
        LatestManusIdea = idea;
        LatestImageUrl = imageUrl;
        LatestContentUrl = contentUrl;
        UpdatedAtUtc = currentDateTime;
    }

    // Records the in-flight Manus generation task (overwrites any previous pending task for this chat).
    public void SetPendingManusTask(string taskId, string? taskUrl, DateTime currentDateTime)
    {
        PendingManusTaskId = taskId;
        PendingManusTaskUrl = taskUrl;
        UpdatedAtUtc = currentDateTime;
    }

    public void ClearPendingManusTask(DateTime currentDateTime)
    {
        PendingManusTaskId = null;
        PendingManusTaskUrl = null;
        UpdatedAtUtc = currentDateTime;
    }

    // Draft -> Archived. Stores the finalized draft content locally (nothing on Facebook yet). Guards the
    // transition so a double finalize cannot overwrite an already-prepared draft.
    public Result MarkArchived(string message, string? link, string? title, DateTime currentDateTime)
    {
        if (Status != MarketingPostStatus.Draft)
            return Result.Failure(MarketingErrors.InvalidStatusForFinalize);

        PostMessage = message;
        PostLink = link;
        Title = title;
        Status = MarketingPostStatus.Archived;
        UpdatedAtUtc = currentDateTime;
        return Result.Success();
    }

    // Archived -> Published. Records the live Facebook post id. Guards so only an archived draft can be published.
    public Result MarkPublished(string facebookPostId, DateTime currentDateTime)
    {
        if (Status != MarketingPostStatus.Archived)
            return Result.Failure(MarketingErrors.InvalidStatusForPublish);

        FacebookPostId = facebookPostId;
        Status = MarketingPostStatus.Published;
        UpdatedAtUtc = currentDateTime;
        return Result.Success();
    }

    public void MarkFailed(DateTime currentDateTime)
    {
        Status = MarketingPostStatus.Failed;
        UpdatedAtUtc = currentDateTime;
    }
}
