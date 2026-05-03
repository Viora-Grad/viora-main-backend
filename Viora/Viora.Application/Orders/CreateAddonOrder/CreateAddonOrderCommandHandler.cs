using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Organizations;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons;

namespace Viora.Application.Orders.CreateAddonOrder;

public class CreateAddonOrderCommandHandler(
    IOrganizationRepository organizationRepository,
    ISubscriptionRepository subscriptionRepository,
    ILimitedFeatureAddonRepository limitedFeatureAddonRepository,
    IAddonOrderRepository addonOrderRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateAddonOrderCommand>
{
    public async Task<Result> Handle(CreateAddonOrderCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with id {request.OrganizationId} not found.");

        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription with id {request.SubscriptionId} not found.");
        if (subscription.OrganizationId != organization.Id)
            return Result.Failure(SubscriptionError.InvalidData);

        var addons = await limitedFeatureAddonRepository.GetByIdsAsync(request.AddonIds, cancellationToken);
        if (addons is null || !addons.Any())
            throw new NotFoundException($"Addons with ids {string.Join(", ", request.AddonIds)} not found.");

        var addon = AddonOrder.CreateAddonOrder(organization.Id, subscription.Id, addons, dateTimeProvider.UtcNow);

        if (addon.IsFailure)
            return Result.Failure(addon.Error);
        addonOrderRepository.Add(addon.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
