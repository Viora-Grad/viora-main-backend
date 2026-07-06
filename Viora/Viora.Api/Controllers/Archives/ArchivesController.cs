using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Archives.CreateArchive;
using Viora.Application.Archives.CreateFolder;
using Viora.Application.Archives.CreateRecord;
using Viora.Application.Archives.CreateTemplate;
using Viora.Application.Archives.CreateTemplateVersion;
using Viora.Application.Archives.DeleteArchive;
using Viora.Application.Archives.DeleteFolder;
using Viora.Application.Archives.DeleteRecord;
using Viora.Application.Archives.DeleteTemplate;
using Viora.Application.Archives.GetArchive;
using Viora.Application.Archives.GetArchives;
using Viora.Application.Archives.GetFolder;
using Viora.Application.Archives.GetFolderTree;
using Viora.Application.Archives.GetRecord;
using Viora.Application.Archives.GetRecordsByFolder;
using Viora.Application.Archives.GetTemplate;
using Viora.Application.Archives.GetTemplateCurrentVersion;
using Viora.Application.Archives.GetTemplatesByFolder;
using Viora.Application.Archives.GetTemplateVersionFields;
using Viora.Application.Archives.PublishTemplateVersion;
using Viora.Application.Archives.SearchRecords;
using Viora.Application.Archives.UpdateArchive;
using Viora.Application.Archives.UpdateFolder;
using Viora.Application.Archives.UpdateRecord;
using Viora.Application.Archives.UpdateTemplate;

namespace Viora.Api.Controllers.Archives;

[Route("api/archives")]
[ApiController]
public class ArchivesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> CreateArchive([FromBody] CreateArchiveRequest request, CancellationToken ct)
    {
        var command = new CreateArchiveCommand(
            request.OrganizationId,
            request.Name,
            request.Description ?? string.Empty,
            request.EnableVersioning,
            request.EnableAttachments,
            request.EnableExport,
            request.EnableAudit);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> GetArchive([FromRoute] Guid id, CancellationToken ct)
    {
        var query = new GetArchiveQuery(id);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpGet("organization/{organizationId:guid}")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> GetArchives([FromRoute] Guid organizationId, CancellationToken ct)
    {
        var query = new GetArchivesQuery(organizationId);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> UpdateArchive([FromRoute] Guid id, [FromBody] UpdateArchiveRequest request, CancellationToken ct)
    {
        var command = new UpdateArchiveCommand(
            id,
            request.Name,
            request.Description ?? string.Empty,
            request.EnableVersioning,
            request.EnableAttachments,
            request.EnableExport,
            request.EnableAudit);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> DeleteArchive([FromRoute] Guid id, CancellationToken ct)
    {
        var command = new DeleteArchiveCommand(id);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpPost("{archiveId:guid}/folders")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> CreateFolder([FromRoute] Guid archiveId, [FromBody] CreateFolderRequest request, CancellationToken ct)
    {
        var command = new CreateFolderCommand(
            archiveId,
            request.ParentFolderId,
            request.Name,
            request.Description ?? string.Empty,
            request.Type ?? "Normal",
            request.Order);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpGet("{archiveId:guid}/folders/{id:guid}")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> GetFolder([FromRoute] Guid archiveId, [FromRoute] Guid id, CancellationToken ct)
    {
        var query = new GetFolderQuery(id);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpGet("{archiveId:guid}/tree")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> GetFolderTree([FromRoute] Guid archiveId, CancellationToken ct)
    {
        var query = new GetFolderTreeQuery(archiveId);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpPut("{archiveId:guid}/folders/{id:guid}")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> UpdateFolder([FromRoute] Guid archiveId, [FromRoute] Guid id, [FromBody] UpdateFolderRequest request, CancellationToken ct)
    {
        var command = new UpdateFolderCommand(id, request.Name, request.Description ?? string.Empty, request.Order);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{archiveId:guid}/folders/{id:guid}")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> DeleteFolder([FromRoute] Guid archiveId, [FromRoute] Guid id, CancellationToken ct)
    {
        var command = new DeleteFolderCommand(id);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpPost("{archiveId:guid}/records")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> CreateRecord([FromRoute] Guid archiveId, [FromBody] CreateRecordRequest request, CancellationToken ct)
    {
        var command = new CreateRecordCommand(
            archiveId,
            request.FolderId,
            request.CustomerId,
            request.AppointmentId,
            request.TemplateId,
            request.TemplateVersion,
            request.Values);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpGet("{archiveId:guid}/records/{id:guid}")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> GetRecord([FromRoute] Guid archiveId, [FromRoute] Guid id, CancellationToken ct)
    {
        var query = new GetRecordQuery(id);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpGet("{archiveId:guid}/folders/{folderId:guid}/records")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> GetRecordsByFolder([FromRoute] Guid archiveId, [FromRoute] Guid folderId, CancellationToken ct)
    {
        var query = new GetRecordsByFolderQuery(folderId);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpGet("{archiveId:guid}/records/search")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> SearchRecords(
        [FromRoute] Guid archiveId,
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? folderId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct)
    {
        var query = new SearchRecordsQuery(archiveId, searchTerm, folderId, fromDate, toDate);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpPut("{archiveId:guid}/records/{id:guid}")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> UpdateRecord([FromRoute] Guid archiveId, [FromRoute] Guid id, [FromBody] UpdateRecordRequest request, CancellationToken ct)
    {
        var command = new UpdateRecordCommand(id, request.Values);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{archiveId:guid}/records/{id:guid}")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> DeleteRecord([FromRoute] Guid archiveId, [FromRoute] Guid id, CancellationToken ct)
    {
        var command = new DeleteRecordCommand(id);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpPost("{archiveId:guid}/templates")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> CreateTemplate([FromRoute] Guid archiveId, [FromBody] CreateTemplateRequest request, CancellationToken ct)
    {
        var command = new CreateTemplateCommand(archiveId, request.FolderId, request.Name, request.Description ?? string.Empty);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpGet("{archiveId:guid}/templates/{id:guid}")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> GetTemplate([FromRoute] Guid archiveId, [FromRoute] Guid id, CancellationToken ct)
    {
        var query = new GetTemplateQuery(id);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpGet("{archiveId:guid}/templates/{id:guid}/current-version")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> GetTemplateCurrentVersion([FromRoute] Guid archiveId, [FromRoute] Guid id, CancellationToken ct)
    {
        var query = new GetTemplateCurrentVersionQuery(id);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpGet("{archiveId:guid}/templates/{id:guid}/versions/{version:int}")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> GetTemplateVersionFields([FromRoute] Guid archiveId, [FromRoute] Guid id, [FromRoute] int version, CancellationToken ct)
    {
        var query = new GetTemplateVersionFieldsQuery(id, version);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpGet("{archiveId:guid}/folders/{folderId:guid}/templates")]
    [Authorize(Policy = "archive:read")]
    public async Task<IActionResult> GetTemplatesByFolder([FromRoute] Guid archiveId, [FromRoute] Guid folderId, CancellationToken ct)
    {
        var query = new GetTemplatesByFolderQuery(folderId);
        var result = await sender.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpPut("{archiveId:guid}/templates/{id:guid}")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> UpdateTemplate([FromRoute] Guid archiveId, [FromRoute] Guid id, [FromBody] UpdateTemplateRequest request, CancellationToken ct)
    {
        var command = new UpdateTemplateCommand(id, request.Name, request.Description ?? string.Empty);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{archiveId:guid}/templates/{id:guid}")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> DeleteTemplate([FromRoute] Guid archiveId, [FromRoute] Guid id, CancellationToken ct)
    {
        var command = new DeleteTemplateCommand(id);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpPost("{archiveId:guid}/templates/{templateId:guid}/versions")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> CreateTemplateVersion([FromRoute] Guid archiveId, [FromRoute] Guid templateId, [FromBody] CreateTemplateVersionRequest request, CancellationToken ct)
    {
        var command = new CreateTemplateVersionCommand(templateId, request.Fields);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{archiveId:guid}/templates/{templateId:guid}/versions/{versionId:guid}/publish")]
    [Authorize(Policy = "archive:write")]
    public async Task<IActionResult> PublishTemplateVersion([FromRoute] Guid archiveId, [FromRoute] Guid templateId, [FromRoute] Guid versionId, CancellationToken ct)
    {
        var command = new PublishTemplateVersionCommand(templateId, versionId);
        var result = await sender.Send(command, ct);
        return result.ToActionResult();
    }
}
