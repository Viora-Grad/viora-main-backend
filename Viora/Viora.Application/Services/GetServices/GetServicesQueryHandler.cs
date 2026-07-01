using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Services;

namespace Viora.Application.Services.GetServices;

internal sealed class GetServicesQueryHandler(IServiceRepository serviceRepository)
    : IQueryHandler<GetServicesQuery, IReadOnlyCollection<GetServicesResponse>>
{
    public async Task<Result<IReadOnlyCollection<GetServicesResponse>>> Handle(
        GetServicesQuery request,
        CancellationToken cancellationToken)
    {
        var services = await serviceRepository.GetByBranchIdAsync(request.BranchId, cancellationToken);

        IReadOnlyCollection<GetServicesResponse> response = services
            .Select(service => new GetServicesResponse(
                service.Id,
                service.BranchId,
                service.Name.Value,
                service.Description.Value,
                service.Type.Value,
                service.Status.ToString(),
                (int)service.Duration.TotalMinutes,
                service.Cost.Amount,
                service.Cost.Currency.Code,
                service.Discount is null
                    ? null
                    : new DiscountResponse(
                        service.Discount.PercentageOutOf100,
                        service.Discount.Reason,
                        service.Discount.StartDateUtc,
                        service.Discount.EndDateUtc)))
            .ToList();

        return Result.Success(response);
    }
}
