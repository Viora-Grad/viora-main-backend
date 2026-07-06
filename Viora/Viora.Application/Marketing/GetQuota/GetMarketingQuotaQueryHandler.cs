using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;
using Viora.Domain.Plans.Features;

namespace Viora.Application.Marketing.GetQuota;

internal sealed class GetMarketingQuotaQueryHandler(
    IFeatureUsageRepository featureUsageRepository,
    IUserContext userContext) : IQueryHandler<GetMarketingQuotaQuery, MarketingQuotaResponse>
{
    public async Task<Result<MarketingQuotaResponse>> Handle(GetMarketingQuotaQuery request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<MarketingQuotaResponse>(MarketingErrors.OrganizationMissing);

        var usage = await featureUsageRepository.GetByOrganizationIdAndFeatureIdAsync(
            organizationId, LimitedFeature.MarketingAiPosts.Id, cancellationToken);

        var response = usage is null
            ? new MarketingQuotaResponse(0, false, null, null)
            : new MarketingQuotaResponse(usage.Quota, true, usage.PeriodStart, usage.PeriodEnd);

        return Result.Success(response);
    }
}
