using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Viora.Api.Extensions;
using Viora.Application.Feedbacks.AddFeedback;
using Viora.Application.Feedbacks.GetFeedbacks;
using Viora.Application.Feedbacks.UpdateFeedback;

namespace Viora.Api.Controllers.Feedbacks;

[Route("api/[controller]")]
[ApiController]
public class FeedbacksController(ISender sender) : ControllerBase
{
    private Guid? UserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;

    [HttpGet]
    public async Task<IActionResult> GetFeedbacks(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFeedbacksQuery(branchId, userId, page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddFeedback(AddFeedbackRequest request, CancellationToken cancellationToken)
    {
        var command = new AddFeedbackCommand(
            request.BranchId,
            UserId ?? Guid.Empty,
            request.ServiceRatingOutOfTen,
            request.BranchOutOfTen,
            request.SystemExperienceOutOfTen,
            request.Comment);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{feedbackId:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateFeedback(Guid feedbackId, UpdateFeedbackRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateFeedbackCommand(
            feedbackId,
            UserId ?? Guid.Empty,
            request.ServiceRatingOutOfTen,
            request.BranchOutOfTen,
            request.SystemExperienceOutOfTen,
            request.Comment);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}