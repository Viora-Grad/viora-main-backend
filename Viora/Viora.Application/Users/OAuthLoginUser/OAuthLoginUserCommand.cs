using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Users.OAuthLoginUser;

public sealed record OAuthLoginUserCommand(string Provider, string ProviderKey) : ICommand;
