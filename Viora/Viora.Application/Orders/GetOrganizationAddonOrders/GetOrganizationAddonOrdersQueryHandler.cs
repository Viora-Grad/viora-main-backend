using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Orders.GetOrganizationAddonOrders;

internal sealed class GetOrganizationAddonOrdersQueryHandler(
    IAddonOrderRepository addonOrderRepository,
    IOrganizationRepository organizationRepository)
    : IQueryHandler<GetOrganizationAddonOrdersQuery, IReadOnlyList<GetOrganizationAddonOrdersResponse>>
{
    public async Task<Result<IReadOnlyList<GetOrganizationAddonOrdersResponse>>> Handle(
        GetOrganizationAddonOrdersQuery request, CancellationToken cancellationToken)
    {
        _ = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {request.OrganizationId} not found");

        var orders = await addonOrderRepository.GetAllByOrganizationIdAsync(request.OrganizationId, cancellationToken);

        IReadOnlyList<GetOrganizationAddonOrdersResponse> result =
            orders.Select(GetOrganizationAddonOrdersResponse.MapToDto).ToList();

        return Result.Success(result);
    }
}
