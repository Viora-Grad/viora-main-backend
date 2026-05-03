using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Plans;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Orders.ChangeSubscriptionOrder;

public class ChangeSubscriptionOrderCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IPlanRepository planRepository,
    ISubscriptionOrderRepository subscriptionOrderRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangeSubscriptionOrderCommand>
{
    public async Task<Result> Handle(ChangeSubscriptionOrderCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription with ID {request.SubscriptionId} not found.");
        var newPlan = await planRepository.GetByIdAsync(request.NewPlanId, cancellationToken)
            ?? throw new NotFoundException($"Plan with ID {request.NewPlanId} not found.");
        if (subscription.PlanId == request.NewPlanId)
            return Result.Failure(SubscriptionError.InvalidPlan);

        var subscriptionOrder = SubscriptionOrder.CreateChangeSubscriptionOrder(subscription.OrganizationId, newPlan, dateTimeProvider.UtcNow);
        if (subscriptionOrder.IsFailure)
            return Result.Failure(subscriptionOrder.Error);
        subscriptionOrderRepository.Add(subscriptionOrder.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
