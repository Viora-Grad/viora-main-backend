using Microsoft.Extensions.Logging;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Marketing.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;
using Viora.Domain.Marketing.Internal;

namespace Viora.Application.Marketing.PollContent;

internal sealed class PollContentCommandHandler(
    IMarketingChatSessionRepository sessionRepository,
    IManusClient manusClient,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ILogger<PollContentCommandHandler> logger) : ICommandHandler<PollContentCommand, MarketingContentStatusResponse>
{
    private const string StatusPending = "Pending";
    private const string StatusReady = "Ready";
    private const string StatusNone = "None";

    public async Task<Result<MarketingContentStatusResponse>> Handle(PollContentCommand request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<MarketingContentStatusResponse>(MarketingErrors.OrganizationMissing);

        var session = await sessionRepository.GetByIdWithMessagesAsync(request.ChatId, cancellationToken);
        if (session is null)
            return Result.Failure<MarketingContentStatusResponse>(MarketingErrors.SessionNotFound);

        if (session.OrganizationId != organizationId)
            return Result.Failure<MarketingContentStatusResponse>(MarketingErrors.NotOwner);

        // Nothing generating: report the last draft (Ready) or that there is none.
        if (string.IsNullOrWhiteSpace(session.PendingManusTaskId))
        {
            var status = session.LatestManusIdea is null ? StatusNone : StatusReady;
            return Result.Success(new MarketingContentStatusResponse(session.Id, status, session.LatestManusIdea));
        }

        var result = await manusClient.GetTaskResultAsync(session.PendingManusTaskId, cancellationToken);
        if (result.IsFailure)
            return Result.Failure<MarketingContentStatusResponse>(result.Error); // keep pending; retryable

        if (!result.Value.Completed)
            return Result.Success(new MarketingContentStatusResponse(session.Id, StatusPending, null));

        // Completed: store the copy (+ any generated image url) as the draft, append an assistant message,
        // and clear the pending task.
        var content = result.Value.Content ?? string.Empty;
        var now = dateTimeProvider.UtcNow;
        session.SetManusIdea(content, result.Value.ImageUrl, now);
        session.AddMessage(MessageRole.Assistant, MessageSource.Manus, content, null, now);
        session.ClearPendingManusTask(now);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Manus content ready for session {SessionId}.", session.Id);
        return Result.Success(new MarketingContentStatusResponse(session.Id, StatusReady, content));
    }
}
