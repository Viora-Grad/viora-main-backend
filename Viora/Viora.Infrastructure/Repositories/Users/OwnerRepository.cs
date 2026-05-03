using Viora.Domain.Users.Owners;

namespace Viora.Infrastructure.Repositories.Users;

internal class OwnerRepository : Repository<Owner>, IOwnerRepository
{
    public OwnerRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }


}
