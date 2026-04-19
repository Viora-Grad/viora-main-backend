using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.PurchaseAddon;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations;
using Viora.Domain.Plans.Addons;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Application.Plans.CreateAddon;

/// <summary>
/// Handles purchasing an add-on for a limited feature within an active subscription.
/// 
/// Responsibilities:
/// - Validates that the organization has an active subscription.
/// - Ensures the requested feature supports add-ons.
/// - Applies additional usage or capacity to the specified feature.
/// - Persists the add-on as part of the subscription context.
/// 
/// Notes:
/// - Enforces business rules around feature limits and add-on eligibility.
/// - Does not bypass feature usage validation; integrates with existing usage system.
/// - May trigger domain events if add-on affects usage lifecycle.
/// </summary>

public class PurchaseAddonCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IOrganizationRepository organizationRepository,
    ILimitedFeatureAddonRepository limitedFeatureAddonRepository,
    IFeatureUsageRepository featureUsageRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<PurchaseAddonCommand>
{
    public async Task<Result> Handle(PurchaseAddonCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription with ID {request.SubscriptionId} not found.");

        if (subscription.Status != SubscriptionStatus.Active)
            return Result.Failure(SubscriptionError.SubscriptionNotActivated);

        var organization = await organizationRepository.GetByIdAsync(subscription.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with ID {subscription.OrganizationId} not found.");

        var featureUsage = await featureUsageRepository.GetByOrganizationIdAndFeatureIdAsync(
            organization.Id,
            request.LimitedFeatureId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Feature usage for Organization ID {organization.Id} and Feature ID {request.LimitedFeatureId} not found.");
        var addon = await limitedFeatureAddonRepository.GetByIdAsync(request.LimitedFeatureId, cancellationToken)
            ?? throw new NotFoundException($"there are not addons for this Feature with id {request.LimitedFeatureId}");
        featureUsage.RechargeQuota(addon.RestoreValue);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();

    }
}
