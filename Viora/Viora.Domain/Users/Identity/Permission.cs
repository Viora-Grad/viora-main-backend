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

    public string? Description { get; set; }

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
    public static readonly Permission AppointmentsCreate = new(52, "appointments:create", "Create appointments");
    public static readonly Permission AppointmentsCancel = new(53, "appointments:cancel", "Cancel appointments");
    public static readonly Permission InvitationsCreate = new(60, "invitations:create", "Create staff members invitations");
    public static readonly Permission InvitationsRead = new(61, "invitations:read", "Read staff members invitations");
    public static readonly Permission InvitationsDelete = new(62, "invitations:delete", "Delete staff members invitations");

    public static readonly Permission ScheduleWrite = new(70, "schedule:write", "Create the schedule of the branches");
    public static readonly Permission ScheduleRead = new(71, "schedule:read", "Read schedule information");
    public static readonly Permission ShiftWrite = new(72, "shift:write", "Create the shift of the Staff");
    public static readonly Permission ShiftRead = new(73, "shift:read", "Read shift information");
    public static readonly Permission FormWrite = new(80, "form:write", "Create the form of the service");
    public static readonly Permission FormRead = new(81, "form:read", "Read the form information");
    public static readonly Permission FormSubmissionRead = new(82, "formSubmission:read", "Read form submission information");
    public static readonly Permission PrescriptionTemplateWrite = new(90, "prescriptionTemplate:write", "Create prescription templates");
    public static readonly Permission PrescriptionTemplateRead = new(91, "prescriptionTamplate:read", "Read prescription templates information");
    public static readonly Permission PrescriptionWrite = new(92, "prescription:write", "Create prescriptions");
    public static readonly Permission PrescriptionRead = new(93, "prescription:read", "Read prescriptions");
    public static readonly Permission OrganizationPerscriptionTemplateRead = new(94, "organizationPrescriptionTemplate:read", "read all organization Perscription Template");
    public static readonly Permission ArchiveWrite = new(100, "archive:write", "Create and modify archived items");
    public static readonly Permission ArchiveRead = new(101, "archive:read", "Read archived items");
    public static readonly Permission RemindersRead = new(110, "reminders:read");
    public static readonly Permission RemindersWrite = new(111, "reminders:write");
    public static readonly Permission StaffWrite = new(120, "staff:write", "update and delete staff members");
    public static readonly Permission InventoryRead = new(130, "inventory:read", "read the inventory items");
    public static readonly Permission InventoryWrite = new(131, "inventory:write", "create, update and delete inventory items");
    public static readonly Permission BranchWrite = new(140, "branch:write", "create, update and delete branches");
    public static readonly Permission DashboardRead = new(150, "dashboard:read", "read the dashboard information");
    public static readonly Permission ServiceWrite = new(160, "service:write", "create, update and delete services");
    public static readonly Permission MarketingWrite = new(170, "marketing:write", "create, update and delete marketing campaigns");
    public static readonly Permission MarketingRead = new(171, "marketing:read", "read marketing campaigns");


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
        AppointmentsCreate,
        AppointmentsCancel,
        InvitationsCreate,
        InvitationsRead,
        InvitationsDelete,
        ScheduleWrite,
        ScheduleRead,
        ShiftRead,
        ShiftWrite,
        FormRead,
        FormWrite,
        PrescriptionRead,
        PrescriptionWrite,
        PrescriptionTemplateWrite,
        OrganizationPerscriptionTemplateRead,
        FormSubmissionRead,
        PrescriptionTemplateRead,
        ArchiveRead,
        ArchiveWrite,
        RemindersRead,
        RemindersWrite,
        StaffWrite,
        InventoryRead,
        InventoryWrite,
        BranchWrite,
        DashboardRead,
        ServiceWrite,
        MarketingWrite,
        MarketingRead
    ];
    #endregion Permission Values
}