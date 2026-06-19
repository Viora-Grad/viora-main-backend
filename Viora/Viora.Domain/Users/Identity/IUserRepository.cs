namespace Viora.Domain.Users.Identity;

public interface IUserRepository
{
    Task<bool> IsUserExistent(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> FindRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, string>> GetNamesDictAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    void AttachRole(Role role);
    void Add(User user);
}
