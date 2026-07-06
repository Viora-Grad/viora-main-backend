using Microsoft.EntityFrameworkCore;
using Viora.Domain.Marketing;

namespace Viora.Infrastructure.Repositories.Marketing;

internal sealed class MetaPageCredentialRepository : Repository<MetaPageCredential>, IMetaPageCredentialRepository
{
    public MetaPageCredentialRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    // Tracked read: callers may mutate (Update/Deactivate) and rely on SaveChanges persisting it.
    public async Task<MetaPageCredential?> GetActiveByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<MetaPageCredential>()
            .FirstOrDefaultAsync(c => c.OrganizationId == organizationId && c.IsActive, cancellationToken);
    }
}
