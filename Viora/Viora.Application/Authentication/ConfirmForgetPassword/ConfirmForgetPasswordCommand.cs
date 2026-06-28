using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Authentication.ConfirmForgetPassword;

public sealed record ConfirmForgetPasswordCommand(string Email, string Otp, string NewPassword, string Ip) : ICommand;
