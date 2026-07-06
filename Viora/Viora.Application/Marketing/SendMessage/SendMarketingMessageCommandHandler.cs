using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Marketing.Abstractions;
using Viora.Application.Marketing.FinalizePost;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;
using Viora.Domain.Marketing.Internal;

namespace Viora.Application.Marketing.SendMessage;

// The marketing agent orchestrator. Detects intent (Groq), then routes:
//   MarketingContent -> Manus (generate copy, store as the draft idea)
//   FinalizePost     -> dispatch FinalizePostCommand via ISender (quota pipeline + Meta create)
// There is no general-chat branch. The finalize branch persists the user message BEFORE dispatching so
// that the finalize step's quota decrement stays isolated (committed only on its own success).
internal sealed class SendMarketingMessageCommandHandler(
    IMarketingChatSessionRepository sessionRepository,
    IMetaPageCredentialRepository credentialRepository,
    IMarketingIntentDetector intentDetector,
    IManusClient manusClient,
    ISender sender,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ILogger<SendMarketingMessageCommandHandler> logger) : ICommandHandler<SendMarketingMessageCommand, MarketingChatResponse>
{
    public async Task<Result<MarketingChatResponse>> Handle(SendMarketingMessageCommand request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<MarketingChatResponse>(MarketingErrors.OrganizationMissing);

        var session = await sessionRepository.GetByIdWithMessagesAsync(request.ChatId, cancellationToken);
        if (session is null)
            return Result.Failure<MarketingChatResponse>(MarketingErrors.SessionNotFound);

        if (session.OrganizationId != organizationId)
            return Result.Failure<MarketingChatResponse>(MarketingErrors.NotOwner);

        var intent = await intentDetector.DetectAsync(request.Message, cancellationToken);
        logger.LogInformation("Marketing message. Session={SessionId} Intent={Intent}", session.Id, intent);

        // Context is the prior turns; the current message is passed separately as the prompt.
        var conversationContext = BuildConversationContext(session);

        var now = dateTimeProvider.UtcNow;
        session.AddMessage(MessageRole.User, MessageSource.User, request.Message, intent, now);

        return intent == MarketingIntent.FinalizePost
            ? await HandleFinalizeAsync(session, organizationId, cancellationToken)
            : await HandleContentAsync(session, request.Message, conversationContext, cancellationToken);
    }

    private async Task<Result<MarketingChatResponse>> HandleContentAsync(
        MarketingChatSession session, string message, string conversationContext, CancellationToken cancellationToken)
    {
        // Manus generates asynchronously: start the task now and return a "generating" status. The client
        // polls the poll-content endpoint, which reads the result and stores it as the draft idea when ready.
        var prompt = string.IsNullOrWhiteSpace(conversationContext)
            ? message
            : $"{conversationContext}\nLatest request: {message}";

        var task = await manusClient.CreateTaskAsync(prompt, cancellationToken);
        if (task.IsFailure)
        {
            logger.LogWarning("Manus task.create failed for session {SessionId}: {Error}", session.Id, task.Error.Name);
            return Result.Failure<MarketingChatResponse>(task.Error);
        }

        var now = dateTimeProvider.UtcNow;
        session.SetPendingManusTask(task.Value.TaskId, task.Value.TaskUrl, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new MarketingChatResponse(
            session.Id,
            session.Status.ToString(),
            "Your content is being generated. Poll for the result.",
            MarketingIntent.MarketingContent.ToString(),
            session.FacebookPostId,
            ContentPending: true));
    }

    private async Task<Result<MarketingChatResponse>> HandleFinalizeAsync(
        MarketingChatSession session, Guid organizationId, CancellationToken cancellationToken)
    {
        // Pre-dispatch guards run BEFORE the quota pipeline so a bad request never consumes a unit.
        if (session.Status != MarketingPostStatus.Draft)
            return Result.Failure<MarketingChatResponse>(MarketingErrors.InvalidStatusForFinalize);

        if (string.IsNullOrWhiteSpace(session.LatestManusIdea))
            return Result.Failure<MarketingChatResponse>(MarketingErrors.NoDraftContent);

        var credential = await credentialRepository.GetActiveByOrganizationAsync(organizationId, cancellationToken);
        if (credential is null)
            return Result.Failure<MarketingChatResponse>(MarketingErrors.CredentialNotFound);

        // Persist the user's finalize message now, before the nested command consumes quota. This keeps the
        // decrement isolated to the finalize handler's own SaveChanges (committed only on its success).
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await sender.Send(new FinalizePostCommand(session.Id, organizationId), cancellationToken);
        if (result.IsFailure)
            return Result.Failure<MarketingChatResponse>(result.Error);

        return Result.Success(new MarketingChatResponse(
            session.Id,
            MarketingPostStatus.Archived.ToString(),
            result.Value.AssistantMessage,
            MarketingIntent.FinalizePost.ToString(),
            result.Value.FacebookPostId,
            ContentPending: false));
    }

    private static string BuildConversationContext(MarketingChatSession session)
    {
        var sb = new StringBuilder();
        foreach (var message in session.Messages.OrderBy(m => m.CreatedAtUtc))
            sb.AppendLine($"{message.Role}: {message.Content}");
        return sb.ToString();
    }
}
