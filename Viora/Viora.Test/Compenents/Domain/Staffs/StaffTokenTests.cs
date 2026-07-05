using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Domain.Staffs;

[TestClass]
public sealed class StaffTokenTests
{
    // ===== Create =====

    [TestMethod]
    public void Create_SetsAllFields()
    {
        // Arrange
        Guid staffId = Guid.NewGuid();
        string tokenHash = "hash123";
        DateTime createdAt = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime expiration = new(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act
        StaffToken token = StaffToken.Create(staffId, tokenHash, createdAt, expiration);

        // Assert
        Assert.AreEqual(staffId, token.StaffId);
        Assert.AreEqual(tokenHash, token.TokenHash);
        Assert.AreEqual(createdAt, token.CreatedAt);
        Assert.AreEqual(expiration, token.Expiration);
        Assert.IsFalse(token.IsRevoked);
        Assert.IsFalse(token.IsUsed);
        Assert.IsNull(token.RevokedAt);
        Assert.IsNull(token.UsedAt);
    }

    // ===== IsValid =====

    [TestMethod]
    public void IsValid_WhenNotRevokedNotUsedNotExpired_ReturnsTrue()
    {
        // Arrange
        DateTime expiration = new(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        StaffToken token = StaffToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow, expiration);
        DateTime now = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        bool result = token.IsValid(now);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsValid_WhenRevoked_ReturnsFalse()
    {
        // Arrange
        DateTime expiration = new(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        StaffToken token = StaffToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow, expiration);
        token.Revoke(DateTime.UtcNow);

        // Act
        bool result = token.IsValid(DateTime.UtcNow);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValid_WhenUsed_ReturnsFalse()
    {
        // Arrange
        DateTime expiration = new(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        StaffToken token = StaffToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow, expiration);
        token.MarkAsUsed(DateTime.UtcNow);

        // Act
        bool result = token.IsValid(DateTime.UtcNow);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValid_WhenNowEqualsExpiration_ReturnsFalse()
    {
        // Arrange
        DateTime expiration = new(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        StaffToken token = StaffToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow, expiration);

        // Act
        bool result = token.IsValid(expiration);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValid_WhenNowAfterExpiration_ReturnsFalse()
    {
        // Arrange
        DateTime expiration = new(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        StaffToken token = StaffToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow, expiration);
        DateTime afterExpiration = expiration.AddSeconds(1);

        // Act
        bool result = token.IsValid(afterExpiration);

        // Assert
        Assert.IsFalse(result);
    }

    // ===== Revoke =====

    [TestMethod]
    public void Revoke_SetsRevokedAt()
    {
        // Arrange
        StaffToken token = StaffToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow, DateTime.UtcNow.AddDays(30));
        DateTime revokedAt = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        token.Revoke(revokedAt);

        // Assert
        Assert.AreEqual(revokedAt, token.RevokedAt);
        Assert.IsTrue(token.IsRevoked);
    }

    [TestMethod]
    public void Revoke_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        StaffToken token = StaffToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow, DateTime.UtcNow.AddDays(30));
        token.Revoke(DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => token.Revoke(DateTime.UtcNow));
    }

    // ===== MarkAsUsed =====

    [TestMethod]
    public void MarkAsUsed_SetsUsedAt()
    {
        // Arrange
        StaffToken token = StaffToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow, DateTime.UtcNow.AddDays(30));
        DateTime usedAt = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        token.MarkAsUsed(usedAt);

        // Assert
        Assert.AreEqual(usedAt, token.UsedAt);
        Assert.IsTrue(token.IsUsed);
    }

    [TestMethod]
    public void MarkAsUsed_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        StaffToken token = StaffToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow, DateTime.UtcNow.AddDays(30));
        token.MarkAsUsed(DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => token.MarkAsUsed(DateTime.UtcNow));
    }
}
