using Microsoft.EntityFrameworkCore;
using Viora.Domain.Organizations.Suspensions;

namespace Viora.Infrastructure.Repositories.Organizations;

internal sealed class SuspensionRepository(ApplicationDbContext context) : Repository<Suspension>(context), ISuspensionRepository
{
    public async Task<Suspension?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Suspension>().FirstOrDefaultAsync<Suspension>(s => s.OwnerId == ownerId, cancellationToken);
    }
}
