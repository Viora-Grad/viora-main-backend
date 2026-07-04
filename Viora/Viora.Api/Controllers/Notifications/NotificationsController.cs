using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Notifications.GetAllNotifications;
using Viora.Application.Notifications.GetNotification;
using Viora.Application.Notifications.SaveDeviceToken;

namespace Viora.Api.Controllers.Notifications;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
    {
        var query = new GetAllNotificationsQuery();
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetNotification(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetNotificationQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost("/devices")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> SaveDeviceToken([FromBody] SaveDeviceTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new SaveDeviceTokenCommand(request.DeviceToken);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
