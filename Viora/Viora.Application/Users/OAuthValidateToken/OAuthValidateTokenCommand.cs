using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.OAuthValidateToken;

public sealed record OAuthValidateTokenCommand(
    string Provider,
    string Token) : ICommand<SocialTokenValidationResult>;
