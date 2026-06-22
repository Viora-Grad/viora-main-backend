namespace Viora.Application.Customers.CreateCustomerProfile;

public sealed record CreateCustomerProfileResponse
(
    Guid CustomerId,
    string? UserName,
    string FirstName,
     string LastName,
    DateOnly DateOfBirth,
    string Gender,
    List<string> PhoneNumbers,
    List<string> EmailAddresses,
    DateTime JoinedAt

);
