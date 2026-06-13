namespace Viora.Domain.Branches;

public interface IBranchRepository
{
    void Add(Branch branch);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
