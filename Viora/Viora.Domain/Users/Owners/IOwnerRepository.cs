namespace Viora.Domain.Users.Owners;

public interface IOwnerRepository
{
    Task<Owner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Owner owner);
}
