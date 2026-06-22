using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.OAuthValidateToken;

public sealed record OAuthValidateTokenCommand(
    string Provider,
    string? Token,
    string? Code,
    string? RedirectUri)
    : ICommand<SocialTokenValidationResult>
{
    public bool IsValid => (Token is not null ^ Code is not null) || (Code is not null && RedirectUri is not null);
    public bool IsToken => Token is not null;
    public bool IsCode => Code is not null;
}
