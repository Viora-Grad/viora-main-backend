using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Domain.Staffs;

[TestClass]
public sealed class StaffRolesTests
{
    // ===== Create =====

    [TestMethod]
    public void Create_ValidInput_SetsAllFields()
    {
        Guid staffId = Guid.NewGuid();
        int roleId = 3;
        Guid assignedBy = Guid.NewGuid();
        DateTime utcNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

        StaffRoles staffRole = StaffRoles.Create(staffId, roleId, assignedBy, utcNow);

        Assert.IsNotNull(staffRole);
        Assert.AreEqual(staffId, staffRole.StaffId);
        Assert.AreEqual(roleId, staffRole.RoleId);
        Assert.AreEqual(assignedBy, staffRole.AssignedByStaffId);
        Assert.AreEqual(utcNow, staffRole.AssignedAt);
        Assert.IsTrue(staffRole.IsActive);
        Assert.IsNull(staffRole.RevokedAt);
    }

    [TestMethod]
    public void Create_EmptyStaffId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            StaffRoles.Create(Guid.Empty, 1, Guid.NewGuid(), DateTime.UtcNow));
    }

    [TestMethod]
    public void Create_InvalidRoleId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            StaffRoles.Create(Guid.NewGuid(), 0, Guid.NewGuid(), DateTime.UtcNow));
    }

    [TestMethod]
    public void Create_NegativeRoleId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            StaffRoles.Create(Guid.NewGuid(), -1, Guid.NewGuid(), DateTime.UtcNow));
    }

    // ===== Revoke =====

    [TestMethod]
    public void Revoke_ActiveRole_SetsRevokedAtAndIsNotActive()
    {
        StaffRoles staffRole = StaffRoles.Create(Guid.NewGuid(), 1, Guid.NewGuid(), DateTime.UtcNow);
        DateTime revokeTime = new(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);

        staffRole.Revoke(revokeTime);

        Assert.AreEqual(revokeTime, staffRole.RevokedAt);
        Assert.IsFalse(staffRole.IsActive);
    }

    [TestMethod]
    public void Revoke_AlreadyRevoked_DoesNotChangeRevokedAt()
    {
        StaffRoles staffRole = StaffRoles.Create(Guid.NewGuid(), 1, Guid.NewGuid(), DateTime.UtcNow);
        DateTime firstRevoke = new(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);
        staffRole.Revoke(firstRevoke);

        staffRole.Revoke(new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc));

        Assert.AreEqual(firstRevoke, staffRole.RevokedAt);
    }
}
