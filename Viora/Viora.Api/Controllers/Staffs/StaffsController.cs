using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Authentication.CreateStaffRole;
using Viora.Application.Authentication.GetOrganizationRoles;
using Viora.Application.Staffs.AssignRoles;
using Viora.Application.Staffs.ChangeStatus;
using Viora.Application.Staffs.CreateStaffInvitation;
using Viora.Application.Staffs.DeleteStaff;
using Viora.Application.Staffs.GetBranchServiceStaffs;
using Viora.Application.Staffs.GetStaffById;
using Viora.Application.Staffs.GetStaffInvitation;
using Viora.Application.Staffs.GetStaffMe;
using Viora.Application.Staffs.SearchStaff;
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
    [HttpGet("organizations/{organizationId:guid}/roles")]
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
    [HttpGet("{staffId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetStaff(Guid staffId, CancellationToken cancellationToken)
    {
        var query = new GetStaffByIdQuery(staffId);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPatch("{StaffId:guid}/status")]
    [Authorize(Policy = "staffs:write")]
    public async Task<IActionResult> UpdateStaffStatus(Guid StaffId, ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeStatusCommand(StaffId, request.Status);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpDelete("{StaffId:guid}")]
    [Authorize(Policy = "staff:write")]
    public async Task<IActionResult> DeleteStaff(Guid StaffId, CancellationToken cancellationToken)
    {
        var command = new DeleteStaffCommand(StaffId);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    // Full profile of the currently authenticated staff member (id resolved from the token):
    // roles + their permissions, assigned branches, and services.
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStaffMeQuery(), cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet("branches/{branchId:guid}/services/{serviceId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetBranchServiceStaffs(Guid branchId, Guid serviceId, CancellationToken cancellationToken)
    {
        var query = new GetBranchServiceStaffsQuery(branchId, serviceId);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> SearchStaff([FromQuery] GetStaffsRequest request, CancellationToken cancellationToken)
    {
        var query = new SearchStaffQuery(
            StaffId: request.StaffId,
            OrganizationId: request.OrganizationId,
            FirstName: request.FirstName,
            LastName: request.LastName,
            RoleIds: request.RoleIds,
            BranchIds: request.BranchIds,
            ServiceIds: request.ServiceIds,
            Statuses: request.Statuses,
            Page: request.Page,
            PageSize: request.PageSize
            );
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}
