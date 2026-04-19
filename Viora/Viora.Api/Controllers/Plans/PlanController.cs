using MediatR;
using Microsoft.AspNetCore.Mvc;
using Viora.Application.Plans.GetFeatureAddon;
using Viora.Application.Plans.GetPlanById;
using Viora.Application.Plans.GetPlans;
using Viora.Application.Plans.PurchaseAddon;

namespace Viora.Api.Controllers.Plans;

[ApiController]
[Route("api/plan")]

public class PlanController : ControllerBase
{
    private readonly ISender _sender;

    public PlanController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Route("/get/{planId}")]
    public async Task<IActionResult> GetPlan(
        Guid planId,
        CancellationToken cancellationToken)
    {
        var query = new GetPlanByIdQuery(planId);
        var result = await _sender.Send(query);
        if (result.IsSuccess)
            return Ok(result.Value);
        else
            return NotFound(result.Error);

    }

    [HttpGet]
    [Route("/getAll")]
    public async Task<IActionResult> GetAllPlans(
        CancellationToken cancellationToken)
    {
        var query = new GetPlansQuery();
        var result = await _sender.Send(query);
        if (result.IsSuccess)
            return Ok(result.Value);
        else
            return NotFound(result.Error);
    }

    [HttpGet]
    [Route("/GetAddon/{LimitedFeatureId}")]
    public async Task<IActionResult> GetFeatureAddon(Guid LimitedFeatureId, CancellationToken cancellationToken)
    {
        var query = new GetFeatureAddonQuery(LimitedFeatureId);
        var result = await _sender.Send(query);
        if (result.IsSuccess)
            return Ok(result.Value);
        return NotFound(result.Error);
    }



    [HttpPut]
    [Route("/Recharge/LimitFeature")]
    public async Task<IActionResult> RechargeLimitedFeature(
        CreateAddonRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PurchaseAddonCommand(request.SubscriptionId, request.LimitedFeatureId, request.LimitedFeatureAddonId);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result);
    }
}
