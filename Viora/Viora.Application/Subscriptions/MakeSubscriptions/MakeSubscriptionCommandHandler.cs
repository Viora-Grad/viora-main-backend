using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.MakeSubscriptions;

public class MakeSubscriptionCommandHandler(
    IPlanRepository planRepository,
    IOrganizationRepository organizationRepository,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<MakeSubscriptionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(MakeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException($"the plan with id {request.PlanId} not found");
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {request.OrganizationId} not found");
        var endDate = plan.PlanPeriod.CalculateEndTime(request.PeriodStart);
        if (endDate.IsFailure)
            return Result.Failure<Guid>(endDate.Error);

        if (endDate.Value < request.PeriodStart)
            return Result.Failure<Guid>(PlanError.InvalidPlanPeriod);
        var result = Subscription.Create(
            request.PlanId,
            request.OrganizationId,
            request.PeriodStart,
            endDate);
        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);
        subscriptionRepository.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);

    }
}
