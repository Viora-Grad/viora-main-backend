using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Authentication.ValidateEmail;

public sealed record ValidateEmailCommand(string Email) : ICommand;
