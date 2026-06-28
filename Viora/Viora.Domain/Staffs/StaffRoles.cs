using Viora.Domain.Users.Identity;

namespace Viora.Domain.Staffs;

public sealed class StaffRoles
{
    public Guid StaffId { get; private set; }
    public int RoleId { get; private set; }

    public RoleScopeType ScopeType { get; private set; }
    public Guid ScopeId { get; private set; } // references the specific scope (e.g., organization id) if applicable, otherwise empty

    public Guid? AssignedByStaffId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt is null;

    public Staff Staff { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private StaffRoles(
        Guid staffId,
        int roleId,
        RoleScopeType scopeType,
        Guid scopeId,
        Guid? assignedByStaffId,
        DateTime assignedAt)
    {
        StaffId = staffId;
        RoleId = roleId;
        ScopeType = scopeType;
        ScopeId = scopeId;
        AssignedByStaffId = assignedByStaffId;
        AssignedAt = assignedAt;
    }
    public static StaffRoles Create(
        Guid staffId,
        int roleId,
        RoleScopeType scopeType,
        Guid scopeId,
        Guid? assignedByStaffId,
        DateTime utcNow)
    {
        if (staffId == Guid.Empty) throw new ArgumentException("StaffId is required.", nameof(staffId));
        if (roleId <= 0) throw new ArgumentException("RoleId must be greater than zero.", nameof(roleId));

        if (scopeType == RoleScopeType.Organization && scopeId == Guid.Empty)
            throw new ArgumentException("Organization scope requires a non-empty ScopeId.", nameof(scopeId));

        if (scopeType != RoleScopeType.Organization && scopeId != Guid.Empty)
            throw new ArgumentException("Only organization scope can use a non-empty ScopeId.", nameof(scopeId));

        return new StaffRoles(staffId, roleId, scopeType, scopeId, assignedByStaffId, utcNow);
    }

    public void Revoke(DateTime utcNow)
    {
        if (RevokedAt is not null) return;
        RevokedAt = utcNow;
    }
}
public enum RoleScopeType
{
    Global = 0,
    CustomerHub = 1,
    OwnerHub = 2,
    Organization = 3
}