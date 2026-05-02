using Viora.Domain.Users.Internal;

namespace Viora.Api.Controllers.Authentication;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender,
    string Email,
    string Password
);