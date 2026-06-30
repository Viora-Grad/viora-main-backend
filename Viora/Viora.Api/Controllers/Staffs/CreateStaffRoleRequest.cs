namespace Viora.Api.Controllers.Staffs;

public sealed record CreateStaffRoleRequest(
    string RoleName,
    string? RoleDescription,
    IEnumerable<int> PermissionsIds
);