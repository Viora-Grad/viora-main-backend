namespace Viora.Domain.Abstraction;

public interface IUnitOfWork
{
    Task<int> SaveChangeAsync(CancellationToken token = default);
}
