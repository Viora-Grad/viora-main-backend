namespace Viora.Domain.Organizations.Suspensions;

public interface ISuspensionRepository
{
    public void Add(Suspension suspension);
    public Task<Suspension?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);
    public Task<Suspension?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    
}
