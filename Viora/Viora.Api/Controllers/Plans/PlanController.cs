using MediatR;
using Microsoft.AspNetCore.Mvc;
using Viora.Application.Plans.GetPlanById;
using Viora.Application.Plans.GetPlans;
using Viora.Application.Subscriptions.GetFeatureAddon;

namespace Viora.Api.Controllers.Plans;

[ApiController]

public class PlanController : ControllerBase
{
    private readonly ISender _sender;

    public PlanController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Route("api/plan/get/{planId}")]
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
    [Route("api/plan/getAll")]
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
    [Route("api/addon/get")]
    public async Task<IActionResult> GetAddons(
        CancellationToken cancellationToken)
    {
        var query = new GetAllAddonsQuery();
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }
}
