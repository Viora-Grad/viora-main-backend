using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.RemoveAddon;

public class RemoveAddonCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveAddonCommand>
{
    public async Task<Result> Handle(RemoveAddonCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdWithAddonAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"this subscription with id {request.SubscriptionId} not found or not active ");
        var addon = subscription.GetAddons().
            FirstOrDefault(a => a.Id == request.SubscriptionAddonId);
        if (addon is null)
            throw new NotFoundException($"the subscription with id {request.SubscriptionId} does not have addon with id {request.SubscriptionAddonId}");
        var result = addon.SoftDelete();
        if (result.IsFailure)
            throw new InvalidOperationException("server failure when remove the addon");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;

    }
}
