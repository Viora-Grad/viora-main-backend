using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Marketing.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Application.Marketing.GetDraftContent;

internal sealed class GetDraftContentQueryHandler(
    IMarketingChatSessionRepository sessionRepository,
    IManusClient manusClient,
    IUserContext userContext) : IQueryHandler<GetDraftContentQuery, MarketingDraftContentResponse>
{
    public async Task<Result<MarketingDraftContentResponse>> Handle(GetDraftContentQuery request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<MarketingDraftContentResponse>(MarketingErrors.OrganizationMissing);

        var session = await sessionRepository.GetByIdAsync(request.ChatId, cancellationToken);
        if (session is null)
            return Result.Failure<MarketingDraftContentResponse>(MarketingErrors.SessionNotFound);

        if (session.OrganizationId != organizationId)
            return Result.Failure<MarketingDraftContentResponse>(MarketingErrors.NotOwner);

        if (string.IsNullOrWhiteSpace(session.LatestContentUrl))
            return Result.Failure<MarketingDraftContentResponse>(MarketingErrors.ContentNotFound);

        // Proxied through the server, which holds the Manus API key (the raw URL is not publicly fetchable).
        var text = await manusClient.DownloadTextAsync(session.LatestContentUrl, cancellationToken);
        if (text.IsFailure)
            return Result.Failure<MarketingDraftContentResponse>(text.Error);

        return Result.Success(new MarketingDraftContentResponse(session.Id, text.Value.Text, text.Value.ContentType));
    }
}
