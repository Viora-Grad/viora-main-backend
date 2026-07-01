using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.LogoutUser;

public sealed record LogoutUserCommand(string RefreshToken) : ICommand;
