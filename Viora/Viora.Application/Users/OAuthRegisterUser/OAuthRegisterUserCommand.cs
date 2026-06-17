using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.OAuthRegisterUser;

public sealed record OAuthRegisterUserCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string Email,
    string Provider,
    string ProviderKey
) : ICommand<Guid>;
