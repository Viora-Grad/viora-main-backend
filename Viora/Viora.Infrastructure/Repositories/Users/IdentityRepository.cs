using Microsoft.EntityFrameworkCore;
using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Repositories.Users;

internal class IdentityRepository : Repository<AuthIdentity>, IIdentityRepository
{
    public IdentityRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    public async Task<AuthIdentity?> GetByProviderAsync(string provider, string providerKey, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<AuthIdentity>().FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderKey == providerKey, cancellationToken);
    }

    public async Task<List<AuthIdentity>?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<AuthIdentity>().Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    }
}
