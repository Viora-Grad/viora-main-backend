using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.RegisterUser;

// TODO put a validator here
public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string Email,
    string Password) : ICommand<Guid>;