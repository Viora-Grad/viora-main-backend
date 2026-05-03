namespace Viora.Domain.Plans;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Plan>> GetAllAsync(CancellationToken cancellationToken);
    public void Add(Plan plan);
}
