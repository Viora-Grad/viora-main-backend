using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Authentication.CreateStaffRole;
using Viora.Application.Authentication.GetOrganizationRoles;
using Viora.Application.Staffs.AssignRoles;
using Viora.Application.Staffs.CreateStaffInvitation;
using Viora.Application.Staffs.GetStaffInvitation;
using Viora.Application.Staffs.UpdateStaffInfo;


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
    [HttpGet("organization/{organizationId:guid}/roles")]
    [Authorize(Policy = "roles:read")]
    public async Task<IActionResult> GetRoles(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var query = new GetOrganizationRolesQuery(organizationId);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost("organizations/{organizationId:guid}/role")]
    [Authorize(Policy = "roles:write")]
    public async Task<IActionResult> CreateStaffRole(Guid organizationId, CreateStaffRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateStaffRoleCommand(
            organizationId,
            request.RoleName,
            request.RoleDescription,
            [.. request.PermissionsIds]);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPatch("{staffId:guid}/role/{roleId:int}")]
    [Authorize(Policy = "roles:write")]
    public async Task<IActionResult> AssignRoleToStaff(Guid staffId, int roleId, CancellationToken cancellationToken)
    {
        var command = new AssignRolesCommand(staffId,
            new List<int> { roleId });
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPatch("{staffId:guid}/role")]
    [Authorize(Policy = "roles:write")]
    public async Task<IActionResult> AssignRoleToStaff(Guid staffId, AssignRolesRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignRolesCommand(staffId,
            [.. request.RoleIds]);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{staffId:guid}")]
    [Authorize(Roles = "Owner")]
    //[Authorize(Policy = "staffs:update")]
    public async Task<IActionResult> UpdateStaffInfo(Guid staffId, UpdateStaffInfoRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateStaffInfoCommand(staffId,
            request.FirstName,
            request.LastName,
            request.Username,
            request.Password,
            request.DateOfBirth,
            request.Gender,
            request.PhoneNumber);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

}
