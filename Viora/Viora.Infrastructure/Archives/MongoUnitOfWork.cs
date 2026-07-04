using Viora.Domain.Abstractions;

namespace Viora.Infrastructure.Archives;

internal class MongoUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken token = default)
    {
        return Task.FromResult(0);
    }
}
