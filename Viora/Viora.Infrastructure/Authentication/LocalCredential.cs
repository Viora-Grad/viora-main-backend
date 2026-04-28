namespace Viora.Infrastructure.Authentication;

internal class LocalCredential(Guid userId, string hashedPassword, int hashVersion = 1)
{
    public Guid UserId { get; private set; } = userId;
    public string HashedPassword { get; private set; } = hashedPassword;
    public int FailedLoginAttempts { get; private set; } // for account lockout policies
    public int HashVersion { get; private set; } = hashVersion; // to support future password hashing algorithm upgrades
    public DateTime? LastChangedAt { get; private set; }

    public void UpdatePassword(string newHashedPassword, DateTime utcNow, int newHashVersion = 1)
    {
        HashedPassword = newHashedPassword;
        HashVersion = newHashVersion;
        LastChangedAt = utcNow;
    }
    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
    }
    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
    }
}
