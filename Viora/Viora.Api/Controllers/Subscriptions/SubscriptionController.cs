using MediatR;
using Microsoft.AspNetCore.Mvc;
using Viora.Application.Subscriptions.ChangeSubscription;
using Viora.Application.Subscriptions.CreateSubscriptions;
using Viora.Application.Subscriptions.RenewSubscriptions;

namespace Viora.Api.Controllers.Subscriptions;

[ApiController]
[Route("api/subscription")]
public class SubscriptionController : ControllerBase
{
    private readonly ISender _sender;
    public SubscriptionController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Route("/CreateSubscription")]
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
    [Route("/RenewSubscription")]
    public async Task<IActionResult> RenewSubscription(
        RenewSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RenewSubscriptionCommand(request.SubscriptionId);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result);
    }

    [HttpPut]
    [Route("/ChangeSubscriptionPlan")]
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
