using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Authentication.ForgetPassword;

public sealed record ForgetPasswordCommand(string Email) : ICommand;
