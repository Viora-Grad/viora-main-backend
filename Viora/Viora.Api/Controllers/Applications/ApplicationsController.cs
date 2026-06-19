using MediatR;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Controllers.Oganizations;
using Viora.Api.Extensions;
using Viora.Application.Organizations.ApproveOnboardRequest;
using Viora.Application.Organizations.GetApplicationDetails;
using Viora.Application.Organizations.RequestOnboard;
using Viora.Application.Organizations.SearchApplications;

namespace Viora.Api.Controllers.Applications;

[Route("api/[controller]")]
[ApiController]
public class ApplicationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> SearchApplications(Guid? id, Guid? ownerId, string? status, string? referralSource, int page = 1, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchApplicationsQuery(id, ownerId, status, referralSource, page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{applicationId:guid}")]
    public async Task<IActionResult> GetApplicationDetails(Guid applicationId, CancellationToken cancellationToken)
    {
        var query = new GetApplicationDetailsQuery(applicationId);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }


    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
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

        return result.ToActionResult(
            createdAtAction: nameof(GetApplicationDetails),
            routeValueFactory: val => new { id = val }
        );
    }

    [HttpPost("{requestId:guid}/approve")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> ApproveOnboardRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var command = new ApproveOnboardRequestCommand(requestId);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(nameof(OrganizationsController.GetOrganizationDetails), val => val);
    }
}