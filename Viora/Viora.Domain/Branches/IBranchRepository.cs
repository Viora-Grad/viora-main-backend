namespace Viora.Domain.Branches;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
