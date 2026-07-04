using Microsoft.EntityFrameworkCore.Storage;
using Viora.Domain.Abstractions;

namespace Viora.Infrastructure;

// Adapts EF Core's IDbContextTransaction to the domain-level ITransaction abstraction so
// Application handlers can hold a pessimistic lock across SaveChanges without referencing EF.
internal sealed class EfTransaction(IDbContextTransaction transaction) : ITransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default) => transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default) => transaction.RollbackAsync(cancellationToken);

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
