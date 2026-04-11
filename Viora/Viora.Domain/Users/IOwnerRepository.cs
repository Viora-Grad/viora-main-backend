namespace Viora.Domain.Users;

public interface IOwnerRepository
{
    Task<Owner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Owner?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(Owner owner);
}
