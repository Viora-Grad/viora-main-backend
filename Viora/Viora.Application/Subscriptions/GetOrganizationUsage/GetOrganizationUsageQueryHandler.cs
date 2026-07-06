using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.GetOrganizationUsage;

internal sealed class GetOrganizationUsageQueryHandler(
    IFeatureUsageRepository featureUsageRepository,
    ISubscriptionRepository subscriptionRepository,
    IPlanRepository planRepository,
    IUserContext userContext) : IQueryHandler<GetOrganizationUsageQuery, OrganizationUsageResponse>
{
    private static readonly Error OrganizationMissing =
        new("Subscription.OrganizationMissing", "No organization is associated with the current user", ErrorCategory.Forbidden);

    public async Task<Result<OrganizationUsageResponse>> Handle(GetOrganizationUsageQuery request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<OrganizationUsageResponse>(OrganizationMissing);

        var usages = await featureUsageRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);

        // The plan's grants give the per-feature limit (usage stores only the remaining amount).
        var subscription = await subscriptionRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
        var plan = subscription is null ? null : await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
        var limits = plan?.PlanLimitedFeatures.ToDictionary(plf => plf.LimitedFeatureId, plf => plf.LimitValue)
                     ?? [];

        var catalog = LimitedFeature.All.ToDictionary(f => f.Id);

        var features = usages
            .Select(u =>
            {
                catalog.TryGetValue(u.LimitedFeatureId, out var feature);
                long? limit = limits.TryGetValue(u.LimitedFeatureId, out var value) ? value : null;
                long? used = limit.HasValue ? Math.Max(0, limit.Value - u.Quota) : null;

                return new FeatureUsageResponse(
                    u.LimitedFeatureId,
                    feature?.Key.value ?? "unknown",
                    feature?.Description.value ?? string.Empty,
                    u.Quota,
                    limit,
                    used,
                    u.PeriodStart,
                    u.PeriodEnd);
            })
            .OrderBy(f => f.Key)
            .ToList();

        return Result.Success(new OrganizationUsageResponse(organizationId, features));
    }
}
