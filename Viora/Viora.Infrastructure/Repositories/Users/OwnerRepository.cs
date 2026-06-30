using Microsoft.EntityFrameworkCore;
using Viora.Domain.Users.Owners;

namespace Viora.Infrastructure.Repositories.Users;

internal class OwnerRepository : Repository<Owner>, IOwnerRepository
{
    public OwnerRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    // The base GetByIdAsync uses FindAsync, which does not load navigation properties,
    // so UserProfile would come back null. Override to eager-load it.
    public override async Task<Owner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Owner>()
            .Include(owner => owner.UserProfile)
            .FirstOrDefaultAsync(owner => owner.Id == id, cancellationToken);
    }
}
