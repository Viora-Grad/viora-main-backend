using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Subscriptions.GetOrganizationSubscriptions;
using Viora.Application.Subscriptions.RemoveAddon;

namespace Viora.Api.Controllers.Subscriptions;

[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly ISender _sender;
    public SubscriptionController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Route("api/subscription/{organizationId}")]
    [Authorize(Roles = "Owner,Admin")]

    public async Task<IActionResult> GetOrganizationSubscriptions(
        Guid organizationId, CancellationToken cancellationToken)
    {
        var query = new GetOrganizationSubscriptionsQuery(organizationId);
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }


    [HttpDelete]
    [Route("api/addon/remove")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> RemoveAddon(
        RemoveAddonRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RemoveAddonCommand(request.SubscriptionId, request.SubscriptionAddonId);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

}
