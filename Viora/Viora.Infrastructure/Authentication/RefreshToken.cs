namespace Viora.Infrastructure.Authentication;

internal class RefreshToken
{
    public Guid Id { get; set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime Expires { get; private set; }

    public DateTime CreationTime { get; private set; }

    public Guid UserId { get; private set; }
    public bool IsRevoked { get; private set; } = false;

    private RefreshToken() { } // for ef core
    private RefreshToken(Guid userId, string tokenHash, DateTime expires, DateTime creationTime)
    {
        UserId = userId;
        TokenHash = tokenHash;
        Expires = expires;
        CreationTime = creationTime;
    }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expires, DateTime creationTime)
    {
        return new RefreshToken(userId, tokenHash, expires, creationTime);
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}