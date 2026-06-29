using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Staffs.CreateStaffInvitation;
using Viora.Application.Staffs.GetStaffInvitation;

namespace Viora.Api.Controllers.Staffs;

[Route("api/[controller]")]
[ApiController]
public class StaffsController(ISender sender) : ControllerBase
{
    [HttpPost("organizations/{organizationId:guid}/invitation")]
    [Authorize(Policy = "invitations:create")] // this policy is a mix of role and permission checks, see the authorization setup in the infrastructure
    public async Task<IActionResult> CreateStaffInvitation(Guid organizationId, CreateStaffInvitationRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateStaffInvitationCommand(
            organizationId,
            [.. request.RoleIds],
            [.. request.BranchIds]);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet("invitation/{tokenId:guid}")]
    [Authorize(Policy = "invitations:read")] // this policy is a mix of role and permission checks, see the authorization setup in the infrastructure
    public async Task<IActionResult> GetStaffInvitation(Guid tokenId, CancellationToken cancellationToken)
    {
        var query = new GetStaffInvitationQuery(tokenId);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }


}
