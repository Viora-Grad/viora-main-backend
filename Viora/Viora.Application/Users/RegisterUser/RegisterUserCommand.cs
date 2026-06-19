using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Users.Internal;

namespace Viora.Application.Users.RegisterUser;

// TODO put a validator here
public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender,
    string Email,
    string Password) : ICommand<Guid>;