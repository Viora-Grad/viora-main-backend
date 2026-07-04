namespace Viora.Domain.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken token = default);

    /// <summary>
    /// Begins an explicit database transaction. Required when a pessimistic lock (UPDLOCK) must be held
    /// across the read, the mutation, and SaveChanges — the lock is released when the transaction ends.
    /// </summary>
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
