namespace Viora.Domain.Users.Identity;

public sealed class Permission
{


    private Permission(int id, string name)
    {
        Id = id;
        Name = name;
    }
    public static Permission Create(int id, string name) => new(id, name);

    public int Id { get; init; }

    public string Name { get; init; }

    #region Permission Values
    public static readonly Permission UsersRead = new(1, "users:read");
    public static readonly Permission UsersWrite = new(2, "users:write");
    public static readonly Permission RolesRead = new(10, "roles:read");
    public static readonly Permission RolesWrite = new(11, "roles:write");
    public static readonly Permission PlansRead = new(20, "plans:read");
    public static readonly Permission PlansWrite = new(21, "plans:write");
    public static readonly Permission SubscriptionsManage = new(30, "subscriptions:manage");
    public static readonly Permission FeaturesRead = new(40, "features:read");
    public static readonly Permission FeaturesWrite = new(41, "features:write");
    public static readonly Permission AppointmentsRead = new(50, "appointments:read");
    public static readonly Permission AppointmentsWrite = new(51, "appointments:write");
    public static readonly Permission ScheduleWrite = new(60, "schedule:write");
    public static readonly Permission ScheduleRead = new(61, "schedule:read");
    public static readonly Permission ShiftWrite = new(62, "shift:write");
    public static readonly Permission ShiftRead = new(63, "shift:read");
    #endregion Permission Values
}