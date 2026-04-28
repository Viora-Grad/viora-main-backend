namespace Viora.Domain.Users.Customers;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Customer customer);
}
