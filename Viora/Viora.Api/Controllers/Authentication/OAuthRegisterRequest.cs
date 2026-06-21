namespace Viora.Api.Controllers.Authentication;

public sealed record OAuthRegisterRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string Email,
    string ProviderKey);