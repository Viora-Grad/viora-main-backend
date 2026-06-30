namespace Viora.Domain.Users.Identity;

public sealed class Permission
{


    private Permission(int id, string name, string? description = null)
    {
        Id = id;
        Name = name;
        Description = description;
    }
    public static Permission Create(int id, string name, string? description = null) => new(id, name, description);

    public int Id { get; init; }

    public string Name { get; init; }

    public string? Description { get; init; }

    #region Permission Values
    public static readonly Permission UsersRead = new(1, "users:read", "Read user information");
    public static readonly Permission UsersWrite = new(2, "users:write", "Create, update, and delete users");
    public static readonly Permission RolesRead = new(10, "roles:read", "Read role information");
    public static readonly Permission RolesWrite = new(11, "roles:write", "Create, update, and delete roles");
    public static readonly Permission PlansRead = new(20, "plans:read", "Read plan information");
    public static readonly Permission PlansWrite = new(21, "plans:write", "Create, update, and delete plans");
    public static readonly Permission SubscriptionsManage = new(30, "subscriptions:manage", "Manage subscriptions");
    public static readonly Permission FeaturesRead = new(40, "features:read", "Read feature information");
    public static readonly Permission FeaturesWrite = new(41, "features:write", "Create, update, and delete features");
    public static readonly Permission AppointmentsRead = new(50, "appointments:read", "Read appointment information");
    public static readonly Permission AppointmentsWrite = new(51, "appointments:write", "Create, update, and delete appointments");
    public static readonly Permission InvitationsCreate = new(60, "invitations:create", "Create staff members invitations");
    public static readonly Permission InvitationsRead = new(61, "invitations:read", "Read staff members invitations");
    public static readonly Permission InvitationsDelete = new(62, "invitations:delete", "Delete staff members invitations");

    public static readonly Permission ScheduleWrite = new(70, "schedule:write");
    public static readonly Permission ScheduleRead = new(71, "schedule:read");
    public static readonly Permission ShiftWrite = new(72, "shift:write");
    public static readonly Permission ShiftRead = new(73, "shift:read");
    public static readonly Permission FormWrite = new(80, "form:write");
    public static readonly Permission FormRead = new(81, "form:read");
    public static readonly Permission PrescriptionTemplateWrite = new(90, "prescription:write");
    public static readonly Permission PerscriptionTemplateRead = new(91, "perscription:read");
    public static readonly Permission PerscriptionWrite = new(92, "perscription:write");
    public static readonly Permission PrescriptionRead = new(93, "perscription:read");

    public static List<Permission> All =>
    [
        UsersRead,
        UsersWrite,
        RolesRead,
        RolesWrite,
        PlansRead,
        PlansWrite,
        SubscriptionsManage,
        FeaturesRead,
        FeaturesWrite,
        AppointmentsRead,
        AppointmentsWrite,
        InvitationsCreate,
        InvitationsRead,
        InvitationsDelete,
        ScheduleWrite,
        ScheduleRead,
        ShiftRead,
        ShiftWrite,
        FormRead,
        FormWrite,
    ];
    #endregion Permission Values
}