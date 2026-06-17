using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Application.AiRag.Orchestrator;
using Viora.Domain.AiRag.Chat;

namespace Viora.Api.Controllers.AiRag;


[ApiController]
[Route("api/ai/chats")]
// [Authorize]
public sealed class ChatsController : ControllerBase
{
    private readonly AiOrchestratorService _orchestrator;
    private readonly ILogger<ChatsController> _logger;

    public ChatsController(AiOrchestratorService orchestrator, ILogger<ChatsController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to the Viora AI assistant.
    ///
    /// Request:  { "message": "How do I cancel?", "sessionId": "..." }
    ///           sessionId is null on the first message, required after.
    ///
    /// Response: { "message": "...", "intent": "...", "sessionId": "...", "actions": [...] }
    ///           The client must store sessionId and send it on every subsequent message.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request, CancellationToken ct)
    {
        // TODO: Uncomment the auth check below once authentication is properly configured
        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        //             ?? User.FindFirstValue("sub");
        // if (string.IsNullOrEmpty(userId)) return Unauthorized();
        // if (!Guid.TryParse(userId, out var userGuid))
        // {
        //     return BadRequest("Invalid user ID format. Expected a GUID.");
        // }

        var userGuid = Guid.Parse("5E3459FC-5193-46D0-A06F-5B7EC15AACF1"); // TODO: Remove this fallback when auth is working

        _logger.LogInformation(
            "Chat request from user {UserGuid}: {{ Message: \"{Message}\", SessionId: \"{SessionId}\" }}",
            userGuid, request.Message, request.SessionId);

        var response = await _orchestrator.HandleAsync(request, userGuid, ct);

        _logger.LogInformation(
            "Chat response for user {UserGuid}: {{ Intent: \"{Intent}\", SessionId: \"{SessionId}\", MessageLength: {Length} }} , Message: \"{Message}\"",
            userGuid, response.Intent, response.SessionId, response.Message?.Length ?? 0, response.Message);

        return Ok(response);
    }
}