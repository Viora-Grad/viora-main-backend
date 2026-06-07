namespace Viora.Infrastructure.Authentication;

public static class Permissions // might be useless after adding those permissions as a policies
{
    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string RolesRead = "roles.read";
    public const string RolesWrite = "roles.write";
    public const string PlansRead = "plans.read";
    public const string PlansWrite = "plans.write";
    public const string SubscriptionsManage = "subscriptions.manage";
    public const string FeaturesRead = "features.read";
    public const string FeaturesWrite = "features.write";
}