using Microsoft.EntityFrameworkCore;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Infrastructure.Repositories.Users;

internal class UserRepository(ApplicationDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
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
    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesDictAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return new Dictionary<Guid, string>();

        return await DbContext.Set<User>()
            .Where(u => idList.Contains(u.Id))
            .Select(u => new
            {
                u.Id,
                FullName = u.PersonalInfo.FirstName + " " + u.PersonalInfo.LastName
            })
            .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

    }
}
