namespace Viora.Api.Controllers.Authentication;

public sealed record RegisterStaffRequest(
    string Token,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string PhoneNumber,
    string Username,
    string Password
);
