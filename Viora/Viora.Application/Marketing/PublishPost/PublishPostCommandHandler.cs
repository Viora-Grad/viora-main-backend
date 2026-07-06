using Microsoft.Extensions.Logging;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Security;
using Viora.Application.Marketing.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;
using Viora.Domain.Marketing.Internal;

namespace Viora.Application.Marketing.PublishPost;

// Publish creates the LIVE Facebook post in one shot from the locally-stored draft: a native photo post when
// the chat has a generated image, otherwise a text/link post. On success the returned post id is recorded and
// the session becomes Published; any failure returns without saving so it can be retried.
internal sealed class PublishPostCommandHandler(
    IMarketingChatSessionRepository sessionRepository,
    IMetaPageCredentialRepository credentialRepository,
    IMetaGraphService metaGraphService,
    IManusClient manusClient,
    ICipher cipher,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ILogger<PublishPostCommandHandler> logger) : ICommandHandler<PublishPostCommand>
{
    public async Task<Result> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure(MarketingErrors.OrganizationMissing);

        var session = await sessionRepository.GetByIdAsync(request.ChatId, cancellationToken);
        if (session is null)
            return Result.Failure(MarketingErrors.SessionNotFound);

        if (session.OrganizationId != organizationId)
            return Result.Failure(MarketingErrors.NotOwner);

        if (session.Status != MarketingPostStatus.Archived)
            return Result.Failure(MarketingErrors.InvalidStatusForPublish);

        if (string.IsNullOrWhiteSpace(session.PostMessage))
            return Result.Failure(MarketingErrors.NoDraftContent);

        var credential = await credentialRepository.GetActiveByOrganizationAsync(organizationId, cancellationToken);
        if (credential is null)
            return Result.Failure(MarketingErrors.CredentialNotFound);

        var accessToken = cipher.Decrypt(credential.AccessToken);

        // Native photo post when the draft has a generated image; otherwise a text/link post. Both are created
        // live (published=true) in a single call.
        Result<MetaPostResult> created;
        if (!string.IsNullOrWhiteSpace(session.LatestImageUrl))
        {
            var image = await manusClient.DownloadImageAsync(session.LatestImageUrl!, cancellationToken);
            if (image.IsFailure)
                return Result.Failure(image.Error);

            created = await metaGraphService.CreatePhotoPostAsync(
                credential.PageId, accessToken, session.PostMessage!,
                image.Value.Bytes, image.Value.FileName, image.Value.ContentType, cancellationToken);
        }
        else
        {
            created = await metaGraphService.CreatePostAsync(
                credential.PageId, accessToken, new MetaPostPayload(session.PostMessage!, session.PostLink), cancellationToken);
        }

        if (created.IsFailure)
            return Result.Failure(created.Error);

        var marked = session.MarkPublished(created.Value.FacebookPostId, dateTimeProvider.UtcNow);
        if (marked.IsFailure)
            return marked;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Marketing post published. Session={SessionId} FacebookPostId={PostId}",
            session.Id, created.Value.FacebookPostId);
        return Result.Success();
    }
}
