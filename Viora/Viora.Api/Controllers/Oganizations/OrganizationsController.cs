using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Viora.Api.Extensions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Organizations.AddToGallery;
using Viora.Application.Organizations.GetLogo;
using Viora.Application.Organizations.GetMyOrganization;
using Viora.Application.Organizations.GetOrganizationBySubdomain;
using Viora.Application.Organizations.GetOrganizationDetails;
using Viora.Application.Organizations.HideOrganization;
using Viora.Application.Organizations.OrganizationNameExists;
using Viora.Application.Organizations.SearchOrganizations;
using Viora.Application.Organizations.SuspendOrganization;
using Viora.Application.Organizations.UpdateLogo;
using Viora.Application.Organizations.UpdateOrganizationProfile;
using Viora.Domain.Organizations.OrganizationDetails.Internal;

namespace Viora.Api.Controllers.Oganizations;

[Route("api/[controller]")]
[ApiController]
public class OrganizationsController(ISender sender) : ControllerBase
{
    private Guid? UserId =>
    Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
        ? userId
        : null;

    [HttpGet]
    public async Task<IActionResult> SearchOrganizations(Guid? id, string? country, string? name, string? serviceType, double minimumRating = 0.0, string? sortBy = null, int page = 1, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchOrganizationsQuery(id, country, name, serviceType, sortBy, minimumRating, OrganizationStatus.Active, page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> GetOrganizationDetails(Guid organizationId, CancellationToken cancellationToken)
    {
        var query = new GetOrganizationDetailsQuery(organizationId);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("subdomain/{organizationSubDomain}")]
    public async Task<IActionResult> GetOrganizationDetailsByName(string organizationSubDomain, CancellationToken cancellationToken)
    {
        var query = new GetOrganizationDetailsBySubdomainQuery(organizationSubDomain);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("me")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetOrganizationDetails(CancellationToken cancellationToken)
    {
        var query = new GetMyOrganizationDetailsQuery((Guid)UserId!);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{organizationId:guid}/gallery/images")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> AddImageToGallery(
    Guid organizationId,
    IFormFileCollection files,
    [FromServices] IStorageSettings storageSettings,
    CancellationToken cancellationToken)
    {
        List<MediaRequest> medias;

        medias = files
            .Select(f => MediaRequest.CreateImage(f.FileName, f.ContentType, f.Length, f.OpenReadStream(), storageSettings.MaxFileSizeBytes))
            .ToList();

        return await AddToGallery(organizationId, medias, cancellationToken);
    }

    [HttpPost("{organizationId:guid}/gallery/documents")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> AddDocToGallery(
    Guid organizationId,
    IFormFileCollection files,
    [FromServices] IStorageSettings storageSettings,
    CancellationToken cancellationToken)
    {
        List<MediaRequest> medias;

        medias = files
            .Select(f => MediaRequest.CreateDocument(f.FileName, f.ContentType, f.Length, f.OpenReadStream(), storageSettings.MaxFileSizeBytes))
            .ToList();

        return await AddToGallery(organizationId, medias, cancellationToken);
    }

    private async Task<IActionResult> AddToGallery(Guid organizationId, List<MediaRequest> medias, CancellationToken cancellationToken)
    {
        var command = new AddToGalleryCommand(organizationId, medias);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{organizationId:guid}/suspend")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SuspendOrganization(Guid organizationId, SuspendOrganizationRequest request, CancellationToken cancellationToken)
    {
        var command = new SuspendOrganizationCommand(organizationId, UserId, request.Reason, request.Notes);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{organizationId:guid}/hide")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> HideOrganization(Guid organizationId, CancellationToken cancellationToken)
    {
        var command = new HideOrganizationCommand(organizationId);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{organizationId:guid}/profile")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateProfile(Guid organizationId, UpdateOrganizationProfileRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateOrganizationProfileCommand(
            organizationId,
            request.SubDomain,
            request.SupportEmail,
            request.BillingEmail,
            request.ServiceDescription,
            request.ServicesProvided,
            request.About);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{organizationId:guid}/logo")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateLogo(Guid organizationId, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var command = new UpdateLogoCommand(organizationId, stream, file.FileName, file.ContentType, file.Length);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }


    [HttpGet("{organizationId:guid}/logo")]
    public async Task<IActionResult> GetBranchGalleryImage(Guid organizationId, CancellationToken cancellationToken)
    {
        var query = new GetLogoQuery(organizationId);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        var file = result.Value;
        return File(file.Content, file.ContentType, file.FileName);
    }


    [HttpGet("exists")]
    public async Task<IActionResult> NameExists([FromQuery] string Name, CancellationToken cancellation)
    {
        var query = new OrganizationNameExistsQuery(Name);
        var result = await sender.Send(query, cancellation);
        return result.ToActionResult();
    }
}
