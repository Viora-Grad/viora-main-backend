using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Controllers.Subscriptions;
using Viora.Api.Extensions;
using Viora.Application.Orders.ChangeSubscriptionOrder;
using Viora.Application.Orders.CreateAddonOrder;
using Viora.Application.Orders.CreateSubscriptionOrder;
using Viora.Application.Orders.GetOrganizationAddonOrders;
using Viora.Application.Orders.GetOrganizationSubscriptionOrders;
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
    [Route("api/order/subscription")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateSubscriptionOrder(
        CreateSubscriptionOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSubscriptionOrderCommand(request.OrganizationId, request.PlanId);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("api/order/subscription/{subscriptionId}/renew")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> RenewSubscriptionOrder(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var command = new RenewSubscriptionOrderCommand(subscriptionId);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }


    [HttpPost]
    [Route("api/order/subscription/change-plan")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> ChangeSubscriptionPlan(
        ChangeSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeSubscriptionOrderCommand(request.SubscriptionId, request.NewPlanId);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("api/order/addon")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateAddonOrder(
        CreateAddAddonOrderRequest createAddAddonRequest,
        CancellationToken cancellationToken)
    {
        var Command = new CreateAddonOrderCommand(createAddAddonRequest.OrganizationId, createAddAddonRequest.SubscriptionId, createAddAddonRequest.Addons);
        var result = await _sender.Send(Command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet]
    [Route("api/organization/{organizationId:guid}/order/addons")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> GetAddons(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var query = new GetOrganizationAddonOrdersQuery(organizationId);
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet]
    [Route("api/organization/{organizationId:guid}/order/subscriptions")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> GetSubscriptions(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var query = new GetOrganizationSubscriptionOrdersQuery(organizationId);
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}
