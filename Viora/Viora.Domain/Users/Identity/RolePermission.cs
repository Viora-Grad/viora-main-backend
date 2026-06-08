namespace Viora.Domain.Users.Identity;

public sealed class RolePermission(int roleId, int permissionId)
{
    public int RoleId { get; set; } = roleId;
    public int PermissionId { get; set; } = permissionId;
}
