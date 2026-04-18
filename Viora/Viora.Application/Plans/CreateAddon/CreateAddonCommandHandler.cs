using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations;
using Viora.Domain.Plans.Addons;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Plans.CreateAddon;

public class CreateAddonCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IOrganizationRepository organizationRepository,
    ILimitedFeatureAddonRepository limitedFeatureAddonRepository,
    IFeatureUsageRepository featureUsageRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateAddonCommand>
{
    public async Task<Result> Handle(CreateAddonCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription with ID {request.SubscriptionId} not found.");
        var organization = await organizationRepository.GetByIdAsync(subscription.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with ID {subscription.OrganizationId} not found.");
        var featureUsage = await featureUsageRepository.GetByOrganizationIdAndFeatureIdAsync(
            organization.Id,
            request.LimitedFeatureId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Feature usage for Organization ID {organization.Id} and Feature ID {request.LimitedFeatureId} not found.");
        var addon = await limitedFeatureAddonRepository.GetByIdAsync(request.LimitedFeatureId, cancellationToken);
        featureUsage.RechargeQuota(addon.RestoreValue);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();

    }
}
