using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.LocalLoginUser;

public sealed record LocalLoginUserCommand(string Email, string Password) : ICommand;

