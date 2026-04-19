using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.ChangeSubscription;

public class ChangeSubscriptionCommandHandler(
    IPlanRepository planRepository,
    ISubscriptionRepository subscriptionRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangeSubscriptionCommand>
{
    public async Task<Result> Handle(ChangeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var newPlan = await planRepository.GetByIdAsync(request.NewPlanId, cancellationToken)
             ?? throw new NotFoundException($"the plan with id{request.NewPlanId} not found");
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"the subscription with id{request.SubscriptionId} not found");
        var startTime = dateTimeProvider.UtcNow;
        var endTimeResult = newPlan.PlanPeriod.CalculateEndTime(startTime);
        if (endTimeResult.IsFailure)
            return Result.Failure(endTimeResult.Error);
        var result = subscription.ChangePlan(subscription.PlanId, subscription.OrganizationId, newPlan.Id, startTime, endTimeResult.Value);
        if (result.IsFailure)
            return Result.Failure(result.Error);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}
