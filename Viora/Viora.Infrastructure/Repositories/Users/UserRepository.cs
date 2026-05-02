using Microsoft.EntityFrameworkCore;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Infrastructure.Repositories.Users;

internal class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public override async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<User>()
            .Include(user => user.Identities)
            .Include(user => user.Roles)
            .ThenInclude(role => role.Permissions)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = new Email(email);
        return await DbContext.Set<User>()
            .AnyAsync(user => user.Email == normalized, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = new Email(email);
        return await DbContext.Set<User>()
            .Include(user => user.Identities)
            .Include(user => user.Roles)
            .ThenInclude(role => role.Permissions)
            .FirstOrDefaultAsync(user => user.Email == normalized, cancellationToken);
    }
}
