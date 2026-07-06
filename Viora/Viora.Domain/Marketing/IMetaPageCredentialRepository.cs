namespace Viora.Domain.Marketing;

public interface IMetaPageCredentialRepository
{
    void Add(MetaPageCredential credential);

    // Returns the single active Facebook Page credential for an organization, or null if none is configured.
    Task<MetaPageCredential?> GetActiveByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);

    // Hard-deletes a credential row.
    void Remove(MetaPageCredential credential);
}
