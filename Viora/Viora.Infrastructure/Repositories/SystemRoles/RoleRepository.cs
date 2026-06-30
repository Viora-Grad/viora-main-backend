using Microsoft.EntityFrameworkCore;
using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Repositories.SystemRoles;

internal class RoleRepository(ApplicationDbContext context) : IRoleRepository
{
    public void Add(Role role)
    {
        context.Set<Role>().Add(role);
    }

    public void AttachRange(IEnumerable<Permission> permissions)
    {
        context.Set<Permission>().AttachRange(permissions);
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Set<Role>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Set<Role>().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<int> roleIds, CancellationToken cancellationToken)
    {
        if (roleIds is null || !roleIds.Any())
            return [];

        return await context.Set<Role>()
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await context.Set<Role>().FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetOrganizationRolesAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return await context.Set<Role>()
            .Where(r => r.TenantId == organizationId)
            .ToListAsync(cancellationToken);
    }

    public void Remove(Role role)
    {
        context.Set<Role>().Remove(role);
    }
}
