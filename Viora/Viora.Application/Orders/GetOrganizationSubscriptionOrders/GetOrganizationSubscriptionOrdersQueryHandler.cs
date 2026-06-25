using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Plans;

namespace Viora.Application.Orders.GetOrganizationSubscriptionOrders;

internal sealed class GetOrganizationSubscriptionOrdersQueryHandler(
    ISubscriptionOrderRepository subscriptionOrderRepository,
    IOrganizationRepository organizationRepository,
    IPlanRepository planRepository)
    : IQueryHandler<GetOrganizationSubscriptionOrdersQuery, IReadOnlyList<GetOrganizationSubscriptionOrdersResponse>>
{
    public async Task<Result<IReadOnlyList<GetOrganizationSubscriptionOrdersResponse>>> Handle(
        GetOrganizationSubscriptionOrdersQuery request, CancellationToken cancellationToken)
    {
        _ = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {request.OrganizationId} not found");

        var orders = await subscriptionOrderRepository.GetAllByOrganizationIdAsync(request.OrganizationId, cancellationToken);

        var planNames = (await planRepository.GetAllAsNoTrackingAsync(cancellationToken))
            .ToDictionary(plan => plan.Id, plan => plan.Name.value);

        IReadOnlyList<GetOrganizationSubscriptionOrdersResponse> result = orders
            .Select(order => GetOrganizationSubscriptionOrdersResponse.MapToDto(
                order, planNames.GetValueOrDefault(order.PlanId, string.Empty)))
            .ToList();

        return Result.Success(result);
    }
}
