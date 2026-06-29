using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Plans;

namespace Viora.Application.Orders.CreateSubscriptionOrder;

public class CreateSubscriptionOrderCommandHandler(
    IOrganizationRepository organizationRepository,
    IPlanRepository planRepository,
    IDateTimeProvider dateTimeProvider,
    ISubscriptionOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateSubscriptionOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSubscriptionOrderCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with id {request.OrganizationId} not found.");
        var plan = await planRepository.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException($"Plan with id {request.PlanId} not found.");
        var subscriptionOrder = SubscriptionOrder.CreateNewSubscriptionOrder(organization.Id, plan, dateTimeProvider.UtcNow);

        if (subscriptionOrder.IsFailure)
            return Result.Failure<Guid>(subscriptionOrder.Error);
        orderRepository.Add(subscriptionOrder.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(subscriptionOrder.Value.Id);

    }
}
