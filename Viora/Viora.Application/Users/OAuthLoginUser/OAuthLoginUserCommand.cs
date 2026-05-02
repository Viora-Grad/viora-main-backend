using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.OAuthLoginUser;

public sealed record OAuthLoginUserCommand(string Provider, string ProviderKey, SocialInput? SocialInput) : ICommand<AuthResult>; //social input will change 
