namespace Viora.Api.Controllers.Authentication;

public sealed record CreateStaffRoleRequest(
    string RoleName,
    string? RoleDescription,
    IEnumerable<int> PermissionsIds
);