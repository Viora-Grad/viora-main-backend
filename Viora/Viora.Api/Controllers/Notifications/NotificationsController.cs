using FirebaseAdmin;
using FirebaseAdmin.Messaging;
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
    [HttpGet("/firebase/test")]
    [AllowAnonymous]
    public async Task<IActionResult> Firebase([FromServices] FirebaseApp? firebaseApp, [FromServices] FirebaseMessaging? messaging)
    {
        if (firebaseApp is null || messaging is null)
            return StatusCode(500, new { ok = false, reason = "Firebase services not registered" });

        try
        {
            var credential = firebaseApp.Options.Credential;
            string? token = null;

            if (credential is Google.Apis.Auth.OAuth2.ITokenAccess ta)
            {
                token = await ta.GetAccessTokenForRequestAsync();
            }
            else if (credential.UnderlyingCredential is Google.Apis.Auth.OAuth2.ITokenAccess uta)
            {
                token = await uta.GetAccessTokenForRequestAsync();
            }
            else
            {
                return StatusCode(500, new { ok = false, reason = "Credential does not implement ITokenAccess" });
            }

            return Ok(new
            {
                ok = true,
                appName = firebaseApp.Name,
                tokenPreview = token is not null ? token[..Math.Min(16, token.Length)] + "..." : null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, reason = "credential error", error = ex.Message });
        }
    }
}
