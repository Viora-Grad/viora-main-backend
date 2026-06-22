using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Customers.CreateCustomerProfile;

public sealed record CreateCustomerProfileCommand(
    string? UserName,
    IEnumerable<string> PhoneNumbers,
    IEnumerable<string> Emails
    ) : ICommand<CreateCustomerProfileResponse>;
