using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Services.AddDiscount;
using Viora.Application.Services.AddService;
using Viora.Application.Services.GetServices;
using Viora.Application.Services.UpdateService;
using Viora.Domain.Shared;

namespace Viora.Api.Controllers.Services;

[Route("api")]
[ApiController]
public class ServicesController(ISender sender) : ControllerBase
{
    [HttpGet("branch/{branchId:guid}/services")]
    [Authorize]
    public async Task<IActionResult> GetServices(Guid branchId, CancellationToken cancellationToken)
    {
        var query = new GetServicesQuery(branchId);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("services")]
    [Authorize]
    public async Task<IActionResult> AddService(AddServiceRequest request, CancellationToken cancellationToken)
    {
        var command = new AddServiceCommand(
            request.BranchId,
            request.Name,
            request.Description,
            request.ServiceType,
            TimeSpan.FromMinutes(request.DurationInMinutes),
            new Money(request.CostAmount, Currency.FromCode(request.Currency)));

        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("services/{serviceId:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateService(Guid serviceId, UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCommand(
            serviceId,
            request.Name,
            request.Description,
            request.ServiceType,
            TimeSpan.FromMinutes(request.DurationInMinutes),
            new Money(request.CostAmount, Currency.FromCode(request.Currency)));

        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("services/{serviceId:guid}/discount")]
    [Authorize]
    public async Task<IActionResult> AddDiscount(Guid serviceId, AddDiscountRequest request, CancellationToken cancellationToken)
    {
        var command = new AddDiscountCommand(
            serviceId,
            request.DiscountOutOf100,
            request.Reason,
            TimeSpan.FromDays(request.DurationInDays));

        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
