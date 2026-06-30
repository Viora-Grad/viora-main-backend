namespace Viora.Domain.Branches;

using Viora.Domain.Abstractions;
using Viora.Domain.Medias;

public interface IBranchRepository
{
    void Add(Branch branch);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Branch>> GetByOrganizationIdAsync(Guid orgId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MediaFile>?> GetMediaByBranchId(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Branch>> ListAsync(ISpecification<Branch> spec, CancellationToken cancellationToken = default);
    Task<long> CountAsync(ISpecification<Branch> spec, CancellationToken cancellationToken = default);

    void Attach(Branch branch);
}
