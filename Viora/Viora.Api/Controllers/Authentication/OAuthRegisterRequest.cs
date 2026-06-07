namespace Viora.Api.Controllers.Authentication;

public sealed record OAuthRegisterRequest(string FirstName, string LastName, DateOnly DateOfBirth, int Gender, string Token);