using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Users.Internal;

namespace Viora.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    int Age,
    UserType UserType) : ICommand<Guid>;

