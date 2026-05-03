using MediatR;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Controllers.Subscriptions;
using Viora.Application.Orders.ChangeSubscriptionOrder;
using Viora.Application.Orders.CreateAddonOrder;
using Viora.Application.Orders.CreateSubscriptionOrder;
using Viora.Application.Orders.RenewSubscriptionOrder;

namespace Viora.Api.Controllers.Orders;


[ApiController]
public class OrderController : ControllerBase
{
    private readonly ISender _sender;

    public OrderController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Route("api/order/subscription/create")]

    public async Task<IActionResult> CreateSubscriptionOrder(
        CreateSubscriptionOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSubscriptionOrderCommand(request.OrganizationId, request.PlanId);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }

    [HttpPost]
    [Route("api/order/subscription/renew/{subscriptionId}")]
    public async Task<IActionResult> RenewSubscriptionOrder(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var command = new RenewSubscriptionOrderCommand(subscriptionId);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }


    [HttpPost]
    [Route("api/order/subscription/changePlan")]
    public async Task<IActionResult> ChangeSubscriptionPlan(
        ChangeSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeSubscriptionOrderCommand(request.SubscriptionId, request.NewPlanId);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }

    [HttpPost]
    [Route("api/order/addon/add")]
    public async Task<IActionResult> CreateAddAddonOrder(
    CancellationToken cancellationToken,
    CreateAddAddonOrderRequest createAddAddonRequest)
    {
        var Command = new CreateAddonOrderCommand(createAddAddonRequest.OrganizationId, createAddAddonRequest.SubscriptionId, createAddAddonRequest.Addons);
        var result = await _sender.Send(Command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }

}
