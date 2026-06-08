using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Seeding.Data;

public static class AuthorizationData
{
    public static IReadOnlyList<Permission> Permissions => new[]
     {
        Permission.UsersRead,
        Permission.Create(2, "users:write"),
        Permission.Create(10, "roles:read"),
        Permission.Create(11, "roles:write"),
        Permission.Create(20, "plans:read"),
        Permission.Create(21, "plans:write"),
        Permission.Create(30, "subscriptions:manage"),
        Permission.Create(40, "features:read"),
        Permission.Create(41, "features:write")
     };
    public static IReadOnlyList<Role> Roles => new[]
    {
        Role.None,
        Role.Registered,
        Role.Owner,
        Role.Admin,
        Role.Customer
    };

    public static IReadOnlyList<RolePermission> RolePermissions => new[]
    {
        new RolePermission(Role.Registered.Id, Permission.UsersRead.Id),
        new RolePermission(Role.Owner.Id, Permission.UsersRead.Id),
        new RolePermission(Role.Owner.Id, 10),
        new RolePermission(Role.Owner.Id, 20),
        new RolePermission(Role.Owner.Id, 30),
        new RolePermission(Role.Owner.Id, 40),
        new RolePermission(Role.Admin.Id, Permission.UsersRead.Id),
        new RolePermission(Role.Admin.Id, 2),
        new RolePermission(Role.Admin.Id, 10),
        new RolePermission(Role.Admin.Id, 11),
        new RolePermission(Role.Admin.Id, 20),
        new RolePermission(Role.Admin.Id, 21),
        new RolePermission(Role.Admin.Id, 30),
        new RolePermission(Role.Admin.Id, 40),
        new RolePermission(Role.Admin.Id, 41)
            // Customer role intentionally left without permissions, can be assigned permissions as needed
    };


}
