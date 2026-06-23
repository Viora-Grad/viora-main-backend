using Microsoft.AspNetCore.Mvc;
using Viora.Application.AiRag.Session;
using Viora.Domain.ChatSessions;

namespace Viora.Api.Controllers.AiRag;

[ApiController]
[Route("api/ai/sessions")]
// [Authorize]
public sealed class SessionsController : ControllerBase
{
    private readonly IChatSessionRepository _repository;
    private readonly GetSessionHistoryQuery _historyQuery;

    public SessionsController(IChatSessionRepository repository, GetSessionHistoryQuery historyQuery)
    {
        _repository = repository;
        _historyQuery = historyQuery;
    }

    /// <summary>
    /// GET /api/ai/sessions?page=1&amp;pageSize=20
    /// Returns paginated session list (no HistoryJson — metadata only).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
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

        pageSize = Math.Clamp(pageSize, 1, 50);
        var sessions = await _repository.GetByUserIdAsync(userGuid, page, pageSize, ct);

        var result = sessions.Select(s => new SessionSummaryDto(s.Id, s.Title, s.CreatedAt, s.LastActiveAt));
        return Ok(result);
    }

    /// <summary>
    /// GET /api/ai/sessions/{sessionId}
    /// Returns full message history for a session (parsed from HistoryJson).
    /// </summary>
    [HttpGet("{sessionId:guid}")]
    public async Task<IActionResult> GetSessionHistory(Guid sessionId, CancellationToken ct)
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

        var result = await _historyQuery.ExecuteAsync(sessionId, userGuid, ct);
        if (result is null) return NotFound();

        return Ok(result);
    }
}