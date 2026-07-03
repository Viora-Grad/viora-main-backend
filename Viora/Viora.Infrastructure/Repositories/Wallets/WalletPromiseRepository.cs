using Microsoft.EntityFrameworkCore;
using Viora.Domain.WalletPromises;

namespace Viora.Infrastructure.Repositories.Wallets;

internal sealed class WalletPromiseRepository : Repository<WalletPromise>, IWalletPromiseRepository
{
    public WalletPromiseRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<WalletPromise?> GetBySourceTransactionIdAsync(Guid sourceTransactionId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<WalletPromise>()
            .FirstOrDefaultAsync(promise => promise.SourceTransactionId == sourceTransactionId, cancellationToken);
    }
}
