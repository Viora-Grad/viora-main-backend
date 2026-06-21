using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Abstractions.Media;
using Viora.Application.LegalPapers.AddLegalPaper;
using Viora.Application.LegalPapers.UpdateLegalPaperStatus;

namespace Viora.Api.Controllers.LegalPapers;

[Route("api/[controller]")]
[ApiController]
public class LegalPapersController(ISender sender) : ControllerBase
{
    private Guid? UserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddLegalPaper(
        [FromForm] AddLegalPaperRequest request,
        [FromServices] IStorageSettings storageSettings,
        CancellationToken cancellationToken)
    {
        MediaRequest media;
        try
        {
            media = request.File.ContentType == "application/pdf"
                ? MediaRequest.CreateDocument(request.File.FileName, request.File.ContentType, request.File.Length, request.File.OpenReadStream(), storageSettings.MaxFileSizeBytes)
                : MediaRequest.CreateImage(request.File.FileName, request.File.ContentType, request.File.Length, request.File.OpenReadStream(), storageSettings.MaxFileSizeBytes);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        var command = new AddLegalPaperCommand(
            request.ApplicationId,
            (Guid)UserId!,
            media,
            request.Type,
            request.OfficialName,
            request.ExpiryDateUtc);

        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{legalPaperId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid legalPaperId,
        UpdateLegalPaperStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLegalPaperStatusCommand(legalPaperId, request.Status);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
