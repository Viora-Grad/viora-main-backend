using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Viora.Api.Webhooks;

[ApiController]
[Route("api/webhooks/kashier")]
public sealed class KashierWebhookController(ILogger<KashierWebhookController> logger) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var headers = string.Join(" | ", Request.Headers.Select(h => $"{h.Key}: {h.Value}"));

        logger.LogInformation("Kashier webhook received.\nHeaders: {Headers}\nBody: {Body}", headers, body);

        return Ok();
    }
}
