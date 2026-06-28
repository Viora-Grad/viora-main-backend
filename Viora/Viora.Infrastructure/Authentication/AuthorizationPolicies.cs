using Microsoft.AspNetCore.Authorization;

namespace Viora.Infrastructure.Authentication;

// for defining authorization policies in the future (could go for another implementation but this can be useful)
public static class AuthorizationPolicies
{
    // roles
    public const string AdminOnly = "AdminOnly";
    public const string OwnerOnly = "OwnerOnly";
    public const string CustomerOnly = "CustomerOnly";
    public const string StaffOnly = "StaffOnly";
    // permissions
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
    public const string RolesRead = "roles:read";
    public const string RolesWrite = "roles:write";
    public const string PlansRead = "plans:read";
    public const string PlansWrite = "plans:write";
    public const string SubscriptionsManage = "subscriptions:manage";
    public const string FeaturesRead = "features:read";
    public const string FeaturesWrite = "features:write";
    public const string AppointmentsRead = "appointments:read";
    public const string AppointmentsWrite = "appointments:write";
    public static readonly string[] All =
    [
        UsersRead, UsersWrite, RolesRead, RolesWrite, PlansRead, PlansWrite,
        SubscriptionsManage, FeaturesRead, FeaturesWrite, AppointmentsRead, AppointmentsWrite
    ];

    public static string Permission(string permission) => $"Permission:{permission}";

    public static void AddPermissionPolicies(this AuthorizationBuilder builder)
    {
        foreach (var permission in All)
            builder.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
    }
}
