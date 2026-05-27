using MediatR;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Organizations.ApproveOnboardRequest;
using Viora.Application.Organizations.GetOrganizationDetails;
using Viora.Application.Organizations.HideOrganization;
using Viora.Application.Organizations.RequestOnboard;
using Viora.Application.Organizations.SearchApplications;
using Viora.Application.Organizations.SearchOrganizations;
using Viora.Application.Organizations.SuspendOrganization;
using Viora.Application.Organizations.UpdateLogo;

namespace Viora.Api.Controllers.Oganizations;

[Route("api/[controller]")]
[ApiController]
public class OrganizationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> SearchOrganizations(Guid? id, string? country, string? name, string? serviceType, double minimumRating = 0.0, int page = 1, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchOrganizationsQuery(id, country, name, serviceType, minimumRating, page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("applications")]
    public async Task<IActionResult> SearchApplications(Guid? id, Guid? ownerId, string? status, string? referralSource, int page = 1, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchApplicationsQuery(id, ownerId, status, referralSource, page, pageSize);
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

    [HttpPost("onboard")]
    public async Task<IActionResult> RequestOnboard(RequestOnboardRequest request, CancellationToken cancellationToken)
    {
        var command = new RequestOnboardCommand(
            request.OwnerId,
            request.CountryId,
            request.ProposedName,
            request.About,
            request.ServiceDescription,
            request.Letter,
            request.ServiceTypes,
            request.ReferralSource,
            request.BillingEmail,
            request.SupportEmail);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("applications/{requestId:guid}/approve")]
    public async Task<IActionResult> ApproveOnboardRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var command = new ApproveOnboardRequestCommand(requestId);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{organizationId:guid}/suspend")]
    public async Task<IActionResult> SuspendOrganization(Guid organizationId, SuspendOrganizationRequest request, CancellationToken cancellationToken)
    {
        var command = new SuspendOrganizationCommand(organizationId, request.SuspendedById, request.Reason, request.Notes);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{organizationId:guid}/hide")]
    public async Task<IActionResult> HideOrganization(Guid organizationId, CancellationToken cancellationToken)
    {
        var command = new HideOrganizationCommand(organizationId);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{organizationId:guid}/logo")]
    public async Task<IActionResult> UpdateLogo(Guid organizationId, [FromBody] UpdateLogoRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateLogoCommand(organizationId, request.MediaId);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
