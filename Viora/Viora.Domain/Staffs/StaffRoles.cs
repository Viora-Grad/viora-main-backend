using Viora.Domain.Users.Identity;

namespace Viora.Domain.Staffs;

public sealed class StaffRoles // this came after a lot of work so it's such a hassle to opt for to not use it for now S
{
    public Guid StaffId { get; private set; }
    public int RoleId { get; private set; }
    public Guid? AssignedByStaffId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt is null;

    public Staff Staff { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private StaffRoles(
        Guid staffId,
        int roleId,
        Guid? assignedByStaffId,
        DateTime assignedAt)
    {
        StaffId = staffId;
        RoleId = roleId;
        AssignedByStaffId = assignedByStaffId;
        AssignedAt = assignedAt;
    }
    public static StaffRoles Create(
        Guid staffId,
        int roleId,
        Guid? assignedByStaffId,
        DateTime utcNow)
    {
        if (staffId == Guid.Empty) throw new ArgumentException("StaffId is required.", nameof(staffId));
        if (roleId <= 0) throw new ArgumentException("RoleId must be greater than zero.", nameof(roleId));


        return new StaffRoles(staffId, roleId, assignedByStaffId, utcNow);
    }

    public void Revoke(DateTime utcNow)
    {
        if (RevokedAt is not null) return;
        RevokedAt = utcNow;
    }
}
