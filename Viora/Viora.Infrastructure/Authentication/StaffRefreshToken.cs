namespace Viora.Infrastructure.Authentication;

public class StaffRefreshToken
{
    public Guid Id { get; set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime Expires { get; private set; }

    public DateTime CreationTime { get; private set; }

    public Guid StaffId { get; private set; }
    public bool IsRevoked { get; private set; } = false;

    private StaffRefreshToken() { } // for ef core
    private StaffRefreshToken(Guid staffId, string tokenHash, DateTime expires, DateTime creationTime)
    {
        StaffId = staffId;
        TokenHash = tokenHash;
        Expires = expires;
        CreationTime = creationTime;
    }

    public static StaffRefreshToken Create(Guid staffId, string tokenHash, DateTime expires, DateTime creationTime)
    {
        return new StaffRefreshToken(staffId, tokenHash, expires, creationTime);
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}
