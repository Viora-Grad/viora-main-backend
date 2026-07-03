using Microsoft.EntityFrameworkCore;
using Viora.Domain.WalletTransactions;
using Viora.Domain.WalletTransactions.Internals;

namespace Viora.Infrastructure.Repositories.Wallets;

internal sealed class WalletTransactionRepository : Repository<WalletTransaction>, IWalletTransactionsRepository
{
    public WalletTransactionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyCollection<WalletTransaction>> GetByWalletIdAsync(Guid walletId, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await DbContext.Set<WalletTransaction>()
            .AsNoTracking()
            .Where(transaction => transaction.WalletId == walletId)
            .OrderByDescending(transaction => transaction.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(TransactionType type, Purpose purpose, string referenceId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<WalletTransaction>()
            .AnyAsync(t => t.Type == type && t.Purpose == purpose && t.ReferenceId == referenceId, cancellationToken);
    }
}
