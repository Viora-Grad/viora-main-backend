namespace Viora.Domain.Users.Identity;

public enum RoleScopeType
{
    Global = 0,
    CustomerHub = 1,
    OwnerHub = 2,
    Organization = 3
}

public sealed class UserRoleAssignment
{
    private UserRoleAssignment() { } // EF

    private UserRoleAssignment(
        Guid userId,
        int roleId,
        RoleScopeType scopeType,
        Guid scopeId,
        Guid? assignedByUserId,
        DateTime assignedAt)
    {
        UserId = userId;
        RoleId = roleId;
        ScopeType = scopeType;
        ScopeId = scopeId;
        AssignedByUserId = assignedByUserId;
        AssignedAt = assignedAt;
    }

    public Guid UserId { get; private set; }
    public int RoleId { get; private set; }

    public RoleScopeType ScopeType { get; private set; }
    public Guid ScopeId { get; private set; } // references the specific scope (e.g., organization id) if applicable, otherwise empty

    public Guid? AssignedByUserId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt is null;

    public AuthAccount User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    public static UserRoleAssignment Create(
        Guid userId,
        int roleId,
        RoleScopeType scopeType,
        Guid scopeId,
        Guid? assignedByUserId,
        DateTime utcNow)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (roleId <= 0) throw new ArgumentException("RoleId must be greater than zero.", nameof(roleId));

        if (scopeType == RoleScopeType.Organization && scopeId == Guid.Empty)
            throw new ArgumentException("Organization scope requires a non-empty ScopeId.", nameof(scopeId));

        if (scopeType != RoleScopeType.Organization && scopeId != Guid.Empty)
            throw new ArgumentException("Only organization scope can use a non-empty ScopeId.", nameof(scopeId));

        return new UserRoleAssignment(userId, roleId, scopeType, scopeId, assignedByUserId, utcNow);
    }

    public void Revoke(DateTime utcNow)
    {
        if (RevokedAt is not null) return;
        RevokedAt = utcNow;
    }
}