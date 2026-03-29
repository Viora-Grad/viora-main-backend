using Viora.Domain.Users;

namespace Viora.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<string> RegisterOwnerAsync(Owner owner, string password, CancellationToken cancellationToken = default);
    Task<string> RegisterCustomerAsync(Customer customer, string password, CancellationToken cancellationToken = default);

    Task<Owner> LoginOwnerAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Customer> LoginCustomerAsync(string email, string password, CancellationToken cancellationToken = default);

}
