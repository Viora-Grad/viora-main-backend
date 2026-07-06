using Viora.Domain.Abstractions;

namespace Viora.Domain.Marketing;

// A tenant's (organization's) Facebook Page credential. AccessToken holds the ENCRYPTED token
// (ICipher-encrypted Base64) — never the plaintext, and never logged.
public sealed class MetaPageCredential : Entity
{
    public Guid OrganizationId { get; private set; }
    public string PageId { get; private set; } = default!;
    public string AccessToken { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private MetaPageCredential() { }

    public static MetaPageCredential Create(Guid organizationId, string pageId, string encryptedAccessToken, DateTime currentDateTime)
    {
        return new MetaPageCredential
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            PageId = pageId,
            AccessToken = encryptedAccessToken,
            IsActive = true,
            CreatedAtUtc = currentDateTime,
            UpdatedAtUtc = currentDateTime
        };
    }

    public void Update(string pageId, string encryptedAccessToken, DateTime currentDateTime)
    {
        PageId = pageId;
        AccessToken = encryptedAccessToken;
        IsActive = true;
        UpdatedAtUtc = currentDateTime;
    }

    public void Deactivate(DateTime currentDateTime)
    {
        IsActive = false;
        UpdatedAtUtc = currentDateTime;
    }
}
