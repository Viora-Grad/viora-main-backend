using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Seeding.Data;

internal static class AuthorizationData
{
    public static IReadOnlyList<Role> Roles => Role.All;

    public static IReadOnlyList<Permission> Permissions =>
    [
        // TODO please register the presmissions in the Permission class and fetch them from there do not define them here
        Permission.UsersRead,
        Permission.Create(2,  "users:write"),
        Permission.Create(10, "roles:read"),
        Permission.Create(11, "roles:write"),
        Permission.Create(20, "plans:read"),
        Permission.Create(21, "plans:write"),
        Permission.Create(30, "subscriptions:manage"),
        Permission.Create(40, "features:read"),
        Permission.Create(41, "features:write"),
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
    ];
}
