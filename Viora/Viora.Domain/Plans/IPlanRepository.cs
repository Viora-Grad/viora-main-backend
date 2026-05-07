namespace Viora.Domain.Plans;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Plan>> GetAllAsNoTrackingAsync(CancellationToken cancellationToken);
    public void Add(Plan plan);
}
