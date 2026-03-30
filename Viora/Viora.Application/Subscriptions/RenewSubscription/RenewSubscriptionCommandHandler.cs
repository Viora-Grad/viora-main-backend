using MediatR;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.RenewSubscription;

public class RenewSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RenewSubscriptionCommand>
{
    public async Task<Result> IRequestHandler<RenewSubscriptionCommand, Result>.Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription with id {request.SubscriptionId} not found.");
        var StarDate = dateTimeProvider.UtcNow;
        var result = subscription.Renew(StarDate, request.Type);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
