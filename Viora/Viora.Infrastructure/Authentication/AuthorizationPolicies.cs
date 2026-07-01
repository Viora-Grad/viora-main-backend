using Microsoft.AspNetCore.Authorization;
using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Authentication;

// for defining authorization policies in the future (could go for another implementation but this can be useful)
public static class AuthorizationPolicies
{
    public static IReadOnlyList<string> All => [.. Permission.All.Select(x => x.Name)];


    // Custom Policies 
    public const string CreateStaffInvitation = "CreateStaffInvitation";

    public static void AddPermissionPolicies(this AuthorizationBuilder builder)
    {
        foreach (var permission in All)
            builder.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
    }
}

