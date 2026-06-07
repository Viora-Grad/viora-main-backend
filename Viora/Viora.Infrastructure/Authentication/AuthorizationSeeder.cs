using Microsoft.EntityFrameworkCore;
using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Authentication;

public static class AuthorizationSeeder
{
    public static void SeedPermissions(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>().HasData(
            Permission.UsersRead,
            new { Id = 2, Name = "users:write" },
            new { Id = 10, Name = "roles:read" },
            new { Id = 11, Name = "roles:write" },
            new { Id = 20, Name = "plans:read" },
            new { Id = 21, Name = "plans:write" },
            new { Id = 30, Name = "subscriptions:manage" },
            new { Id = 40, Name = "features:read" },
            new { Id = 41, Name = "features:write" }
        );
    }
    public static void SeedRoles(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            //Role.None,
            //Role.Registered,
            //Role.Owner,
            Role.Admin,
            Role.Customer
        );
    }
    public static void SeedRolePermissions(this ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<RolePermission>().HasData(
            new { RoleId = Role.Registered.Id, PermissionId = Permission.UsersRead.Id },
            new { RoleId = Role.Owner.Id, PermissionId = Permission.UsersRead.Id },
            new { RoleId = Role.Owner.Id, PermissionId = 10 },
            new { RoleId = Role.Owner.Id, PermissionId = 20 },
            new { RoleId = Role.Owner.Id, PermissionId = 30 },
            new { RoleId = Role.Owner.Id, PermissionId = 40 },
            new { RoleId = Role.Admin.Id, PermissionId = Permission.UsersRead.Id },
            new { RoleId = Role.Admin.Id, PermissionId = 2 },
            new { RoleId = Role.Admin.Id, PermissionId = 10 },
            new { RoleId = Role.Admin.Id, PermissionId = 11 },
            new { RoleId = Role.Admin.Id, PermissionId = 20 },
            new { RoleId = Role.Admin.Id, PermissionId = 21 },
            new { RoleId = Role.Admin.Id, PermissionId = 30 },
            new { RoleId = Role.Admin.Id, PermissionId = 40 },
            new { RoleId = Role.Admin.Id, PermissionId = 41 }
            // Customer role intentionally left without permissions, can be assigned permissions as needed
        );
    }


}
