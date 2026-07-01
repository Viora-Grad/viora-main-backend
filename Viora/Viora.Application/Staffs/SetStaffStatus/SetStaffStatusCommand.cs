namespace Viora.Application.Staffs.SetStaffStatus;

public sealed record SetStaffStatusCommand(
    Guid StaffId,
    string Status
);