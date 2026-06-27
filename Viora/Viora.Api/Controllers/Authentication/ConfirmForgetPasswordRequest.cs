namespace Viora.Api.Controllers.Authentication;

public sealed record ConfirmForgetPasswordRequest(string Email, string Otp, string NewPassword);