using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string Password,
    int Age) : ICommand<Guid>;

