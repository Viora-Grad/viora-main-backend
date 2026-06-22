using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.OAuthLoginUser;

public sealed record OAuthLoginUserCommand(string Provider, string? Token, string? Code, string? RedirectUri) : ICommand<AuthResult> //social input will change
{
    public bool IsToken => !string.IsNullOrEmpty(Token);
    public bool IsCode => !string.IsNullOrEmpty(Code);
}
