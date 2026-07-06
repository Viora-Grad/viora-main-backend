using System.Text;
using Microsoft.Extensions.Logging;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Marketing.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;
using Viora.Domain.Marketing.Internal;

namespace Viora.Application.Marketing.FinalizePost;

// Finalize prepares the post LOCALLY: Groq turns the latest Manus idea into the final copy, which is stored on
// the session (message/link/title; the generated image url is already there) and the session is marked
// Archived. Nothing is created on Facebook here — publish does that. Quota timing contract still holds: the
// pipeline consumes -1 before this handler, and the single success-only SaveChanges commits both the archived
// draft and the decrement; any failure returns without saving so the decrement is discarded.
internal sealed class FinalizePostCommandHandler(
    IMarketingChatSessionRepository sessionRepository,
    IMarketingPostJsonBuilder postJsonBuilder,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ILogger<FinalizePostCommandHandler> logger) : ICommandHandler<FinalizePostCommand, FinalizePostResult>
{
    public async Task<Result<FinalizePostResult>> Handle(FinalizePostCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetByIdWithMessagesAsync(request.ChatId, cancellationToken);
        if (session is null)
            return Result.Failure<FinalizePostResult>(MarketingErrors.SessionNotFound);

        if (session.OrganizationId != request.OrganizationId)
            return Result.Failure<FinalizePostResult>(MarketingErrors.NotOwner);

        // Defensive idempotency guard: a double-submit is normally stopped in the orchestrator before the
        // quota pipeline runs, but if two calls race this second guard prevents re-preparing (and rolls back
        // the just-consumed unit by not saving).
        if (session.Status != MarketingPostStatus.Draft)
            return Result.Failure<FinalizePostResult>(MarketingErrors.InvalidStatusForFinalize);

        if (string.IsNullOrWhiteSpace(session.LatestManusIdea))
            return Result.Failure<FinalizePostResult>(MarketingErrors.NoDraftContent);

        var conversationContext = BuildConversationContext(session);

        var built = await postJsonBuilder.BuildAsync(session.LatestManusIdea!, conversationContext, cancellationToken);
        if (built.IsFailure)
            return Result.Failure<FinalizePostResult>(built.Error);

        var post = built.Value;
        var now = dateTimeProvider.UtcNow;

        var archived = session.MarkArchived(post.Message, post.Link, post.Title, now);
        if (archived.IsFailure)
            return Result.Failure<FinalizePostResult>(archived.Error);

        var confirmation = $"Your post draft{(post.Title is null ? "" : $" \"{post.Title}\"")} is ready. Publish it whenever you want it live on your Facebook Page.";
        session.AddMessage(MessageRole.Assistant, MessageSource.Groq, confirmation, MarketingIntent.FinalizePost, now);

        // The single success-only save: commits the archived draft, the assistant message, AND the quota -1.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Marketing draft prepared (archived locally). Session={SessionId} Org={Org} HasImage={HasImage}",
            session.Id, request.OrganizationId, !string.IsNullOrWhiteSpace(session.LatestImageUrl));

        return Result.Success(new FinalizePostResult(session.Id, null, post.Title, confirmation));
    }

    private static string BuildConversationContext(MarketingChatSession session)
    {
        var sb = new StringBuilder();
        foreach (var message in session.Messages.OrderBy(m => m.CreatedAtUtc))
            sb.AppendLine($"{message.Role}: {message.Content}");
        return sb.ToString();
    }
}
