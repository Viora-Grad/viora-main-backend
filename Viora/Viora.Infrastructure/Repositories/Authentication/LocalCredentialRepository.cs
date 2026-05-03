using Microsoft.EntityFrameworkCore;
using Viora.Infrastructure.Authentication;

namespace Viora.Infrastructure.Repositories.Authentication;

internal class LocalCredentialRepository(ApplicationDbContext dbContext)
{
    public Task<LocalCredential?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<LocalCredential>().FirstOrDefaultAsync(lc => lc.UserId == userId, cancellationToken);
    }
    public void Add(LocalCredential localCredential)
    {
        dbContext.Set<LocalCredential>().Add(localCredential);

    }
}
