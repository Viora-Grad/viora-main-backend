using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.AiRag.Orchestrator;
using Viora.Domain.AiRag.Chat;

namespace Viora.Api.Controllers.AiRag;


[ApiController]
[Route("api/ai/chats")]
[Authorize]
public sealed class ChatsController(
    AiOrchestratorService orchestrator,
    ILogger<ChatsController> logger,
    IUserContext userContext) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request, CancellationToken ct)
    {
        var userGuid = userContext.UserId;

        logger.LogInformation(
            "Chat request from user {UserGuid}: {{ Message: \"{Message}\", SessionId: \"{SessionId}\" }}",
            userGuid, request.Message, request.SessionId);

        var response = await orchestrator.HandleAsync(request, userGuid, ct);

        logger.LogInformation(
            "Chat response for user {UserGuid}: {{ Intent: \"{Intent}\", SessionId: \"{SessionId}\", MessageLength: {Length} }} , Message: \"{Message}\"",
            userGuid, response.Intent, response.SessionId, response.Message?.Length ?? 0, response.Message);

        return Ok(response);
    }
}