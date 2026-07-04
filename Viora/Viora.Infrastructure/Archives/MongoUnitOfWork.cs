using MongoDB.Driver;
using Viora.Domain.Abstractions;

namespace Viora.Infrastructure.Archives;

internal class MongoUnitOfWork(MongoDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken token = default)
    {
        return Task.FromResult(0);
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var session = await context.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();
        return new MongoTransaction(session);
    }
}

internal class MongoTransaction : ITransaction
{
    private readonly IClientSessionHandle _session;

    public MongoTransaction(IClientSessionHandle session)
    {
        _session = session;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _session.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _session.AbortTransactionAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        _session.Dispose();
        return ValueTask.CompletedTask;
    }
}
