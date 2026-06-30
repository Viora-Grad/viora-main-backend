using Viora.Domain.Abstractions;

namespace Viora.Domain.Staffs;

public sealed class StaffToken : Entity
{
    public Guid StaffId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime Expiration { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsRevoked => RevokedAt.HasValue;
    public DateTime? UsedAt { get; private set; }
    public bool IsUsed => UsedAt.HasValue;

    public Staff Staff { get; private set; } // Navigation property to the Staff entity

    public bool IsValid(DateTime now) => !IsRevoked && !IsUsed && now < Expiration;
    private StaffToken() { } // For EF Core
    private StaffToken(Guid staffId, string tokenHash, DateTime createdAt, DateTime expiration)
    {
        StaffId = staffId;
        TokenHash = tokenHash;
        Expiration = expiration;
        CreatedAt = createdAt;
    }
    public static StaffToken Create(Guid staffId, string tokenHash, DateTime createdAt, DateTime expiration)
    {
        return new StaffToken(staffId, tokenHash, createdAt, expiration);
    }
    public void Revoke(DateTime revokedAt)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Token is already revoked.");
        RevokedAt = revokedAt;
    }
    public void MarkAsUsed(DateTime usedAt)
    {
        if (IsUsed)
            throw new InvalidOperationException("Token is already used.");
        UsedAt = usedAt;
    }
}
