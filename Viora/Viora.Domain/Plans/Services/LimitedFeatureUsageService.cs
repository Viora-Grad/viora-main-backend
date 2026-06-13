using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Domain.Plans.Services;

public class LimitedFeatureUsageService(
    ISubscriptionRepository subscriptionRepository,
    IFeatureUsageRepository featureUsageRepository
    ) : ILimitedFeatureUsageService
{
    public async Task<Result> CheckLimitAsync(Guid organizationId, Guid limitedFeatureId, int delta, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);

        if (subscription is null)
            return Result.Failure(SubscriptionError.OrganizationNotSubscribed);
        if (subscription.Status != SubscriptionStatus.Active)
            return Result.Failure(SubscriptionError.SubscriptionNotActivated);

        var organizationFeatureUsage = await featureUsageRepository.GetByOrganizationIdAndFeatureIdAsync(organizationId, limitedFeatureId, cancellationToken);
        if (organizationFeatureUsage is null)
            return Result.Failure(SubscriptionError.FeatureUsageNotFound);

        // only enforce limit for consumption (negative delta)
        if (delta < 0 && organizationFeatureUsage.Quota + delta < 0)
            return Result.Failure(SubscriptionError.LimitExceeded);

        return Result.Success();
    }

    public async Task<Result> ConsumeLimit(Guid organizationId, Guid limitedFeatureId, int delta, CancellationToken cancellationToken)
    {
        var organizationFeatureUsage = await featureUsageRepository.GetByOrganizationIdAndFeatureIdAsync(organizationId, limitedFeatureId, cancellationToken);
        if (organizationFeatureUsage is null)
            return Result.Failure(SubscriptionError.FeatureUsageNotFound);

        organizationFeatureUsage.Consume(delta);
        return Result.Success();
    }
}
