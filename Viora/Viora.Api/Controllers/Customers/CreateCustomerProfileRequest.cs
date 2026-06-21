namespace Viora.Api.Controllers.Customers;

public sealed record CreateCustomerProfileRequest(
    string? UserName,
    IEnumerable<string> PhoneNumbers,
    IEnumerable<string> Emails
    );