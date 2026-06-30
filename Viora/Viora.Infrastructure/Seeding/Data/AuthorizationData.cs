using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Seeding.Data;

internal static class AuthorizationData
{
    public static IReadOnlyList<Role> Roles => Role.All;

    public static IReadOnlyList<Permission> Permissions => Permission.All;

    public static IReadOnlyList<RolePermission> RolePermissions =>
    [
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.UsersRead.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.UsersWrite.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.RolesRead.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.RolesWrite.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.PlansRead.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.PlansWrite.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.FeaturesRead.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.FeaturesWrite.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.SubscriptionsManage.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.InvitationsRead.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.InvitationsCreate.Id },
        new() { RoleId = Role.Owner.Id, PermissionId = Permission.InvitationsDelete.Id },

        new() { RoleId = Role.Admin.Id, PermissionId = Permission.UsersRead.Id },
        new() { RoleId = Role.Admin.Id, PermissionId = Permission.UsersWrite.Id  },
        new() { RoleId = Role.Admin.Id, PermissionId = Permission.RolesRead.Id },
        new() { RoleId = Role.Admin.Id, PermissionId = Permission.RolesWrite.Id },
        new() { RoleId = Role.Admin.Id, PermissionId = Permission.PlansRead.Id },
        new() { RoleId = Role.Admin.Id, PermissionId = Permission.PlansWrite.Id },
        new() { RoleId = Role.Admin.Id, PermissionId = Permission.FeaturesRead.Id },
        new() { RoleId = Role.Admin.Id, PermissionId = Permission.FeaturesWrite.Id },
        new() { RoleId = Role.Customer.Id, PermissionId = Permission.UsersRead.Id },
    ];
}
