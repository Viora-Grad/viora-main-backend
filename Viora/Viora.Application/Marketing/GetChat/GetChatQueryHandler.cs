using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Application.Marketing.GetChat;

internal sealed class GetChatQueryHandler(
    IMarketingChatSessionRepository sessionRepository,
    IUserContext userContext) : IQueryHandler<GetChatQuery, MarketingChatDetailResponse>
{
    public async Task<Result<MarketingChatDetailResponse>> Handle(GetChatQuery request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<MarketingChatDetailResponse>(MarketingErrors.OrganizationMissing);

        var session = await sessionRepository.GetByIdWithMessagesAsync(request.ChatId, cancellationToken);
        if (session is null)
            return Result.Failure<MarketingChatDetailResponse>(MarketingErrors.SessionNotFound);

        if (session.OrganizationId != organizationId)
            return Result.Failure<MarketingChatDetailResponse>(MarketingErrors.NotOwner);

        var messages = session.Messages
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new MarketingMessageResponse(
                m.Id,
                m.Role.ToString(),
                m.Source.ToString(),
                m.Content,
                m.DetectedIntent?.ToString(),
                m.CreatedAtUtc))
            .ToList();

        var response = new MarketingChatDetailResponse(
            session.Id,
            session.Status.ToString(),
            session.Title,
            session.FacebookPostId,
            session.PostMessage,
            session.PostLink,
            !string.IsNullOrWhiteSpace(session.LatestImageUrl),
            session.CreatedAtUtc,
            session.UpdatedAtUtc,
            messages);

        return Result.Success(response);
    }
}
