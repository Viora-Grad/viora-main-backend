using Microsoft.EntityFrameworkCore;
using Viora.Domain.Users.Identity;
namespace Viora.Infrastructure.Repositories.Users;

internal class UserRepository : Repository<User>, IUserRepository
{

    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<User>().AnyAsync(user => user.Email.Value == email, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return DbContext.Set<User>()
            .Include(user => user.Identities)
            .Include(user => user.Roles)
            .Include(user => user.Contacts)
            .FirstOrDefaultAsync(user => user.Email.Value == email, cancellationToken);
    }
}
