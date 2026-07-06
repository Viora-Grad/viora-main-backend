using Microsoft.Extensions.Logging;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Security;
using Viora.Application.Marketing.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Application.Marketing.ConnectMetaPage;

// Facebook Login "connect a Page" flow (mirrors the Postman steps):
//   1. Exchange the short-lived user token (AuthCode) for a long-lived one via the App credentials.
//   2. Call GET /me/accounts with that long-lived token and find the Page whose id == PageId; take its token.
//   3. Encrypt the Page token and upsert it as the organization's active credential.
// The resulting Page token inherits the long-lived user token's lifetime (effectively non-expiring), which is
// why the exchange in step 1 matters. No token is ever logged or returned to the client.
internal sealed class ConnectMetaPageCommandHandler(
    IMetaGraphService metaGraphService,
    IMetaPageCredentialRepository credentialRepository,
    IUserContext userContext,
    ICipher cipher,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ILogger<ConnectMetaPageCommandHandler> logger) : ICommandHandler<ConnectMetaPageCommand>
{
    public async Task<Result> Handle(ConnectMetaPageCommand request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure(MarketingErrors.OrganizationMissing);

        // 1. Short-lived user token -> long-lived user token.
        var longLivedResult = await metaGraphService.ExchangeForLongLivedUserTokenAsync(request.AuthCode, cancellationToken);
        if (longLivedResult.IsFailure)
            return Result.Failure(longLivedResult.Error);

        // 2. Resolve the Page's own access token from the user's managed pages.
        var pageTokenResult = await metaGraphService.GetPageAccessTokenAsync(longLivedResult.Value, request.PageId, cancellationToken);
        if (pageTokenResult.IsFailure)
            return Result.Failure(pageTokenResult.Error);

        // 3. Encrypt at rest and upsert the active credential for this organization.
        var now = dateTimeProvider.UtcNow;
        var encryptedToken = cipher.Encrypt(pageTokenResult.Value);

        var existing = await credentialRepository.GetActiveByOrganizationAsync(organizationId, cancellationToken);
        if (existing is null)
        {
            var credential = MetaPageCredential.Create(organizationId, request.PageId, encryptedToken, now);
            credentialRepository.Add(credential);
        }
        else
        {
            existing.Update(request.PageId, encryptedToken, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Connected Facebook Page {PageId} for organization {OrganizationId}.", request.PageId, organizationId);
        return Result.Success();
    }
}
