using Viora.Domain.Abstractions;

namespace Viora.Domain.Staffs;

public static class StaffErrors
{
    public static readonly Error StaffNotFound = new("Staff.NotFound", "Staff not found.", ErrorCategory.NotFound);
    public static readonly Error StaffAlreadyExists = new("Staff.AlreadyExists", "Staff already exists.", ErrorCategory.Conflict);
    public static readonly Error InvalidStaffEmail = new("Staff.InvalidEmail", "Invalid staff email.", ErrorCategory.Validation);
    public static readonly Error InvalidStaffPhoneNumber = new("Staff.InvalidPhoneNumber", "Invalid staff phone number.", ErrorCategory.Validation);
    public static readonly Error InvalidStaffRole = new("Staff.InvalidRole", "Invalid staff role.", ErrorCategory.Validation);
    public static readonly Error StaffNotActive = new("Staff.NotActive", "Staff is not active.", ErrorCategory.Validation);
    public static readonly Error StaffAlreadyActive = new("Staff.AlreadyActive", "Staff is already active.", ErrorCategory.Validation);
    public static readonly Error InvalidStaffInstance = new("Staff.InvalidInstance", "Invalid staff instance.", ErrorCategory.Validation);
    public static readonly Error StaffAlreadySuspended = new("Staff.AlreadySuspended", "Staff is already suspended.", ErrorCategory.Validation);
    public static readonly Error InvalidInvitationToken = new("Staff.InvalidInvitationToken", "Invalid invitation token due to expiration or usage.", ErrorCategory.Validation);

}
