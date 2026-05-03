using Viora.Domain.Abstractions;

namespace Viora.Application.Abstractions.Authentication;

public static class AuthenticationErrors
{
    public static Error InvalidCredentials => new("Authentication.InvalidCredentials",
        "The provided credentials are invalid.", ErrorCategory.Validation);
    public static Error UserNotFound => new("Authentication.UserNotFound",
        "No user was found with the provided information.", ErrorCategory.NotFound);

    public static Error InvalidToken => new("Authentication.InvalidToken",
        "The provided token is invalid or has expired.", ErrorCategory.Validation);
}
