using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Viora.Api.Extensions;
using Viora.Application.Abstractions.Media;
using Viora.Application.LegalPapers.AddLegalPaper;
using Viora.Application.LegalPapers.GetLegalPaperFile;
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

    // Sensitive media is served through its owning legal paper, not a generic media-by-id
    // endpoint: the handler authorizes against the paper's application owner (admins are
    // privileged) so the underlying media id is never an access primitive.
    [HttpGet("{legalPaperId:guid}/file")]
    [Authorize]

    public async Task<IActionResult> GetFile(Guid legalPaperId, CancellationToken cancellationToken)
    {
        var roles = User.Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Select(x => x.Value)
            .ToList();

        bool isAdmin = roles.Any(role => role.Contains("Admin"));

        ; var query = new GetLegalPaperFileQuery(
            legalPaperId,
            (Guid)UserId!,
            isAdmin);

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        var file = result.Value;
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpPut("{legalPaperId:guid}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(
        Guid legalPaperId,
        UpdateLegalPaperStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLegalPaperStatusCommand(legalPaperId, (Guid)UserId!, request.Status);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
