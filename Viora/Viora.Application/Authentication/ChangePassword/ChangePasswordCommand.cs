using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Authentication.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string Ip) : ICommand;
