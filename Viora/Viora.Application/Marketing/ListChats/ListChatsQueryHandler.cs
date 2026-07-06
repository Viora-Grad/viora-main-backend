using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Application.Marketing.ListChats;

internal sealed class ListChatsQueryHandler(
    IMarketingChatSessionRepository sessionRepository,
    IUserContext userContext) : IQueryHandler<ListChatsQuery, IReadOnlyList<MarketingChatSummaryResponse>>
{
    public async Task<Result<IReadOnlyList<MarketingChatSummaryResponse>>> Handle(ListChatsQuery request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<IReadOnlyList<MarketingChatSummaryResponse>>(MarketingErrors.OrganizationMissing);

        var sessions = await sessionRepository.ListByOrganizationAsync(organizationId, cancellationToken);

        var response = sessions
            .Select(s => new MarketingChatSummaryResponse(
                s.Id,
                s.Title,
                s.Status.ToString(),
                s.FacebookPostId,
                s.CreatedAtUtc,
                s.UpdatedAtUtc))
            .ToList();

        return Result.Success<IReadOnlyList<MarketingChatSummaryResponse>>(response);
    }
}
