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
    public async Task<Result> CheckLimitAsync(Guid organizationId, Guid limitedFeatureId, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
        if (subscription is null)
            return Result.Failure(SubscriptionError.OrganizationNotSubscribed);
        if (subscription.Stauts != SubscriptionStatus.Active)
            return Result.Failure(SubscriptionError.SubscriptionNotActivated);
        var organizationFeatureUsage = await featureUsageRepository.GetByOrganizationIdAndFeatureIdAsync(organizationId, limitedFeatureId, cancellationToken);
        if (organizationFeatureUsage.Quota < 1)
            return Result.Failure(SubscriptionError.LimitExceeded);
        return Result.Success();

    }

    public async void ConsumeLimit(Guid organizationId, Guid limitedFeatureId, CancellationToken cancellationToken)
    {
        var organizationFeatureUsage = await featureUsageRepository.GetByOrganizationIdAndFeatureIdAsync(organizationId, limitedFeatureId, cancellationToken);
        organizationFeatureUsage.Consume();
    }


}
