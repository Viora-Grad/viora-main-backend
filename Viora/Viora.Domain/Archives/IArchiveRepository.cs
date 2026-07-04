namespace Viora.Domain.Archives;

public interface IArchiveRepository
{
    Task<Archive?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Archive>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    void Add(Archive archive);
    void Update(Archive archive);
    void Remove(Archive archive);
}
