namespace Viora.Domain.Users.Identity;

public sealed class RolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    #region Values

    // define the Permission collection for Roles then add that list to the All collection
    public static List<RolePermission> OwnerPermissions =>
        Permission.All.Select(p => new RolePermission { RoleId = Role.Owner.Id, PermissionId = p.Id }).ToList();



    public static IReadOnlyList<RolePermission> All =>
        [.. OwnerPermissions];

    #endregion

}
