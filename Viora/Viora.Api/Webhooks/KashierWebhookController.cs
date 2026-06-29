using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Application.Payments.Webhooks;
using Viora.Domain.Abstractions;

namespace Viora.Api.Webhooks;

[ApiController]
[Route("api/webhooks/kashier")]
public sealed class KashierWebhookController(ISender sender, ILogger<KashierWebhookController> logger) : ControllerBase
{
    private const string SignatureHeader = "x-kashier-signature";

    [HttpPost("subscription")]
    [AllowAnonymous]
    public Task<IActionResult> Subscription(CancellationToken cancellationToken)
        => HandleAsync(WebhookKind.Subscription, cancellationToken);

    [HttpPost("addon")]
    [AllowAnonymous]
    public Task<IActionResult> Addon(CancellationToken cancellationToken)
        => HandleAsync(WebhookKind.Addon, cancellationToken);

    private async Task<IActionResult> HandleAsync(WebhookKind kind, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers.TryGetValue(SignatureHeader, out var values) ? values.ToString() : string.Empty;

        var result = await sender.Send(new HandleKashierWebhookCommand(kind, body, signature), cancellationToken);

        // Invalid signature is the only case we reject; everything else returns 200 so Kashier
        // stops retrying (the handler has already logged any internal anomaly).
        if (result.IsFailure && result.Error.Category == ErrorCategory.Unauthorized)
        {
            logger.LogWarning("Kashier {Kind} webhook rejected: {Error}.", kind, result.Error.Name);
            return Unauthorized();
        }

        return Ok();
    }
}
