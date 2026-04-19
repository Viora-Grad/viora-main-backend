using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.RenewSubscriptions;

/// <summary>
/// Handles renewal of an existing subscription.
/// 
/// Responsibilities:
/// - Validates that the subscription is eligible for renewal.
/// - Extends the subscription period.
/// - Maintains or refreshes subscription configuration.
/// - Raises a domain event to reset or reinitialize feature usage for the new period.
/// 
/// Notes:
/// - Renewal may reset usage limits depending on business rules.
/// - Delegates feature usage reset to domain event handler.
/// </summary>

public class RenewSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IPlanRepository planRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RenewSubscriptionCommand>
{
    public async Task<Result> Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Subscription with id {request.SubscriptionId} not found.");
        var plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken)
            ?? throw new NotFoundException($"the plan with id {subscription.PlanId} not found");
        var startDate = dateTimeProvider.UtcNow;
        var endDate = plan.PlanPeriod.CalculateEndTime(startDate);
        if (endDate.IsFailure)
            return Result.Failure(endDate.Error);
        var result = subscription.Renew(startDate, endDate.Value);
        if (result.IsFailure)
            return Result.Failure(result.Error);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
