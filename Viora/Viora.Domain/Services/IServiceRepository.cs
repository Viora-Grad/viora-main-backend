namespace Viora.Domain.Services;

public interface IServiceRepository
{
    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
