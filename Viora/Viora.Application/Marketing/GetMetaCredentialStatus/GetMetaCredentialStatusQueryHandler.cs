using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Application.Marketing.GetMetaCredentialStatus;

internal sealed class GetMetaCredentialStatusQueryHandler(
    IMetaPageCredentialRepository credentialRepository,
    IUserContext userContext) : IQueryHandler<GetMetaCredentialStatusQuery, MetaCredentialStatusResponse>
{
    public async Task<Result<MetaCredentialStatusResponse>> Handle(GetMetaCredentialStatusQuery request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<MetaCredentialStatusResponse>(MarketingErrors.OrganizationMissing);

        var existing = await credentialRepository.GetActiveByOrganizationAsync(organizationId, cancellationToken);

        var response = existing is null
            ? new MetaCredentialStatusResponse(false, null)
            : new MetaCredentialStatusResponse(true, existing.PageId);

        return Result.Success(response);
    }
}
