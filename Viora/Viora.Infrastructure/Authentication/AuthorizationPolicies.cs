namespace Viora.Infrastructure.Authentication;

// for defining authorization policies in the future (could go for another implementation but this can be useful)
public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string OwnerOnly = "OwnerOnly";
    public const string CustomerOnly = "CustomerOnly";
    public const string StaffOnly = "StaffOnly";
}
