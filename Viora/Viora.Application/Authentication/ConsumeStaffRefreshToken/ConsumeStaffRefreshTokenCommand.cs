using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Authentication.ConsumeStaffRefreshToken;

public sealed record ConsumeStaffRefreshTokenCommand(string RefreshToken) : ICommand<AuthResult>;
