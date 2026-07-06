using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Analytics.GetDashboard;

namespace Viora.Api.Controllers.Analytics;

[Route("api/analytics")]
[ApiController]
[Authorize]
public class AnalyticsController(ISender sender) : ControllerBase
{
    // Server-rendered dashboard for the caller's organization (org id from the JWT). Returns a self-contained
    // HTML page. Required: from, to; optional: granularity=day|week|month (default day).
    // Example: GET /api/analytics/dashboard?from=2026-06-01&to=2026-07-01&granularity=week
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? granularity,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDashboardQuery(from, to, granularity), cancellationToken);

        return result.IsSuccess
            ? Content(DashboardHtmlRenderer.Render(result.Value), "text/html")
            : result.ToActionResult();
    }
}
