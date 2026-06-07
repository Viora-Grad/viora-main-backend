using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Authentication.ConsumeRefreshToken;

public sealed record ConsumeRefreshTokenCommand(string RefreshToken) : ICommand<AuthResult>;
