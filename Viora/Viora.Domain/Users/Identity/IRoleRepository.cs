namespace Viora.Domain.Users.Identity;

public interface IRoleRepository
{
    void Add(Role role);
    Task<IEnumerable<Role>> GetOrganizationRolesAsync(Guid organizationId, CancellationToken cancellationToken);
    Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<int> roleIds, CancellationToken cancellationToken);
    Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken);
    void AttachRange(IEnumerable<Permission> permissions);
    void Remove(Role role);
}
