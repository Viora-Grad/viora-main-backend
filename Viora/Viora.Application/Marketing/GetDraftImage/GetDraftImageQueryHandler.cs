using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Marketing.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Application.Marketing.GetDraftImage;

internal sealed class GetDraftImageQueryHandler(
    IMarketingChatSessionRepository sessionRepository,
    IManusClient manusClient,
    IUserContext userContext) : IQueryHandler<GetDraftImageQuery, MarketingImageResponse>
{
    public async Task<Result<MarketingImageResponse>> Handle(GetDraftImageQuery request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<MarketingImageResponse>(MarketingErrors.OrganizationMissing);

        var session = await sessionRepository.GetByIdAsync(request.ChatId, cancellationToken);
        if (session is null)
            return Result.Failure<MarketingImageResponse>(MarketingErrors.SessionNotFound);

        if (session.OrganizationId != organizationId)
            return Result.Failure<MarketingImageResponse>(MarketingErrors.NotOwner);

        if (string.IsNullOrWhiteSpace(session.LatestImageUrl))
            return Result.Failure<MarketingImageResponse>(MarketingErrors.ImageNotFound);

        // Proxied through the server, which holds the Manus API key (the raw URL is not publicly fetchable).
        var image = await manusClient.DownloadImageAsync(session.LatestImageUrl, cancellationToken);
        if (image.IsFailure)
            return Result.Failure<MarketingImageResponse>(image.Error);

        return Result.Success(new MarketingImageResponse(image.Value.Bytes, image.Value.ContentType, image.Value.FileName));
    }
}
