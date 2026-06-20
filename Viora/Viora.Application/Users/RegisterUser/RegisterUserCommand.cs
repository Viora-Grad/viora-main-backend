using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string Email,
    string Password) : ICommand<Guid>;