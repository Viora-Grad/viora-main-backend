using Viora.Domain.Abstractions;

namespace Viora.Domain.Users;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "The user with the specified identifier was not found", ErrorCategory.NotFound);
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "The provided credentials are invalid", ErrorCategory.Validation);

}
