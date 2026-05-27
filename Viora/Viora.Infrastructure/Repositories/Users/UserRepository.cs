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
    /// <summary>
    /// used to bail early in case user does not exist in other modules
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> IsUserExistent(Guid id, CancellationToken cancellationToken = default)
    {
        return DbContext.Set<User>().AnyAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// retrives the roles from the database to track it, since the roles are staticly made and not tracked
    /// an alt is to seed the roles on run, get them from the database in a readonly collection and inject them for usage
    /// instead of using the static members (look at how I made country)
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Role?> FindRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return DbContext.Set<Role>().FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }
}
