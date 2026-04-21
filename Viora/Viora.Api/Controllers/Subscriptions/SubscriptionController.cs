using MediatR;
using Microsoft.AspNetCore.Mvc;
using Viora.Application.Subscriptions.ChangeSubscription;
using Viora.Application.Subscriptions.CreateSubscriptions;
using Viora.Application.Subscriptions.RenewSubscriptions;

namespace Viora.Api.Controllers.Subscriptions;

[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly ISender _sender;
    public SubscriptionController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Route("api/subscription/create")]
    public async Task<IActionResult> CreateSubscriptions(
        CreateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSubscriptionCommand(request.PlanId, request.OrganizationID);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPut]
    [Route("api/subscription/renew")]
    public async Task<IActionResult> RenewSubscription(
        RenewSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RenewSubscriptionCommand(request.SubscriptionId);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }

    [HttpPut]
    [Route("api/subscription/change")]
    public async Task<IActionResult> ChangeSubscriptionPlan(
        ChangeSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeSubscriptionCommand(request.SubscriptionId, request.NewPlanId);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result);
    }
}
