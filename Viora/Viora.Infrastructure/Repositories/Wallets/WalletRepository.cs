using Microsoft.EntityFrameworkCore;
using Viora.Domain.Wallets;
using Viora.Domain.Wallets.Internals;

namespace Viora.Infrastructure.Repositories.Wallets;

internal sealed class WalletRepository : Repository<Wallet>, IWalletRepository
{
    public WalletRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    // No-tracking: callers use this only to resolve the wallet id; the authoritative tracked load
    // happens under lock via GetForUpdateAsync so the balance can't be stale.
    public async Task<Wallet?> GetByOwnerAsync(Guid ownerId, WalletType type, CancellationToken cancellationToken = default)
    {
        return type == WalletType.Branch
            ? await DbContext.Set<Wallet>().AsNoTracking().FirstOrDefaultAsync(w => w.BranchId == ownerId, cancellationToken)
            : await DbContext.Set<Wallet>().AsNoTracking().FirstOrDefaultAsync(w => w.UserId == ownerId, cancellationToken);
    }

    public async Task<bool> ExistsForOwnerAsync(Guid ownerId, WalletType type, CancellationToken cancellationToken = default)
    {
        return type == WalletType.Branch
            ? await DbContext.Set<Wallet>().AnyAsync(w => w.BranchId == ownerId, cancellationToken)
            : await DbContext.Set<Wallet>().AnyAsync(w => w.UserId == ownerId, cancellationToken);
    }

    // Pessimistic lock: held until the surrounding transaction commits. FromSqlInterpolated parameterizes
    // the id safely; SELECT * returns every mapped column (including the Balance complex-property columns).
    public async Task<Wallet?> GetForUpdateAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Wallet>()
            .FromSqlInterpolated($@"
            SELECT 
                Id, 
                UserId, 
                BranchId, 
                OpenedAtUtc,
                BalanceAmount AS [Balance_Amount],
                BalanceCurrency AS [Balance_Currency_Code],
                CurrencyCode AS [Currency_Code]
            FROM Wallets WITH (UPDLOCK, ROWLOCK) 
            WHERE Id = {walletId}")
            .FirstOrDefaultAsync(cancellationToken);
    }
}
