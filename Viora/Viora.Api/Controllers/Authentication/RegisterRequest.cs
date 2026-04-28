namespace Viora.Api.Controllers.Authentication;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    int Gender,
    string Email,
    string Password
);