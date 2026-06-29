using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Authentication.CreateStaffRole;

public sealed record CreateStaffRoleCommand(
    Guid OrganizationId,
    string RoleName,
    string? RoleDescription,
    List<int> PermissionsIds) : ICommand<int>
{
}
