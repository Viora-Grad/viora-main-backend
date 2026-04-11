namespace Viora.Domain.Users;

public interface ICustomerRepository
{
    Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Customer customer);
}
