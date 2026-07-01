using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.AiRag.Session;
using Viora.Domain.ChatSessions;

namespace Viora.Api.Controllers.AiRag;

[ApiController]
[Route("api/ai/sessions")]
[Authorize]
public sealed class SessionsController(
    IChatSessionRepository repository,
    GetSessionHistoryQuery historyQuery,
    IUserContext userContext) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userGuid = userContext.UserId;

        pageSize = Math.Clamp(pageSize, 1, 50);
        var sessions = await repository.GetByUserIdAsync(userGuid, page, pageSize, ct);

        var result = sessions.Select(s => new SessionSummaryDto(s.Id, s.Title, s.CreatedAt, s.LastActiveAt));
        return Ok(result);
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<IActionResult> GetSessionHistory(Guid sessionId, CancellationToken ct)
    {
        var userGuid = userContext.UserId;

        var result = await historyQuery.ExecuteAsync(sessionId, userGuid, ct);
        if (result is null) return NotFound();

        return Ok(result);
    }
}