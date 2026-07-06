using MediatR;
using Microsoft.Extensions.Logging;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Marketing.SendMessage;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Application.Marketing.StartChat;

internal sealed class StartChatCommandHandler(
    IMarketingChatSessionRepository sessionRepository,
    ISender sender,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ILogger<StartChatCommandHandler> logger) : ICommandHandler<StartChatCommand, StartChatResponse>
{
    public async Task<Result<StartChatResponse>> Handle(StartChatCommand request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<StartChatResponse>(MarketingErrors.OrganizationMissing);

        var session = MarketingChatSession.Create(organizationId, userContext.UserId, dateTimeProvider.UtcNow);
        sessionRepository.Add(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(request.FirstMessage))
            return Result.Success(new StartChatResponse(session.Id, null));

        // Route the first prompt through the same orchestration path as any other message.
        var reply = await sender.Send(new SendMarketingMessageCommand(session.Id, request.FirstMessage), cancellationToken);
        if (reply.IsFailure)
        {
            // The session was created; surface it so the client can retry via the messages endpoint.
            logger.LogWarning("Start-chat first message failed for session {SessionId}: {Error}", session.Id, reply.Error.Name);
            return Result.Success(new StartChatResponse(session.Id, null));
        }

        return Result.Success(new StartChatResponse(session.Id, reply.Value));
    }
}
