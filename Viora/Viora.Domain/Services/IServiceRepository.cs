namespace Viora.Domain.Services;

public interface IServiceRepository
{
    void Add(Service service);
    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Service>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);
}
