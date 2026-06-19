using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Seeding.Data;

internal static class AuthorizationData
{
    public static IReadOnlyList<Role> Roles => Role.All;

    public static IReadOnlyList<Permission> Permissions =>
    [
        // TODO please register the presmissions in the Permission class and fetch them from there do not define them here
        Permission.UsersRead,
        Permission.UsersWrite,
        Permission.RolesRead,
        Permission.RolesWrite,
        Permission.PlansRead,
        Permission.PlansWrite,
        Permission.FeaturesRead,
        Permission.FeaturesWrite,
        Permission.SubscriptionsManage
    ];

    public static IReadOnlyList<RolePermission> RolePermissions =>
    [
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.UsersRead.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = 10 },
        new() { RoleId = Role.Owner.Id, PermissionId = 20 },
        new() { RoleId = Role.Owner.Id, PermissionId = 30 },
        new() { RoleId = Role.Owner.Id, PermissionId = 40 },
        new() { RoleId = Role.Admin.Id, PermissionId = Permission.UsersRead.Id },
        new() { RoleId = Role.Admin.Id, PermissionId = 2  },
        new() { RoleId = Role.Admin.Id, PermissionId = 10 },
        new() { RoleId = Role.Admin.Id, PermissionId = 11 },
        new() { RoleId = Role.Admin.Id, PermissionId = 20 },
        new() { RoleId = Role.Admin.Id, PermissionId = 21 },
        new() { RoleId = Role.Admin.Id, PermissionId = 30 },
        new() { RoleId = Role.Admin.Id, PermissionId = 40 },
        new() { RoleId = Role.Admin.Id, PermissionId = 41 },
        new() { RoleId = Role.Customer.Id, PermissionId = Permission.UsersRead.Id },
    ];
}
