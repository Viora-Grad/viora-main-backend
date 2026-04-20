using Microsoft.EntityFrameworkCore;
using Viora.Domain.Users;

namespace Viora.Infrastructure.Repositories.Users;

internal class OwnerRepository : Repository<Owner>, IOwnerRepository
{
    public OwnerRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Owner?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Owner>()
            .Include(owner => owner.AcceptedTerms)
            .Include(owner => owner.User)
            .FirstOrDefaultAsync(owner => owner.UserId == userId, cancellationToken);

    }
}
