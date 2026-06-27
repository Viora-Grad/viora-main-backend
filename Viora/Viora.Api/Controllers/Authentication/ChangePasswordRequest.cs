namespace Viora.Api.Controllers.Authentication;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
