using Viora.Domain.Abstractions;

namespace Viora.Domain.Users;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "The user with the specified identifier was not found", ErrorCategory.NotFound);
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "The provided credentials are invalid", ErrorCategory.Validation);
    public static readonly Error EmailInUse = new("User.EmailInUse", "The provided email is already in use", ErrorCategory.Validation);
    public static readonly Error EmptyField = new("User.EmptyField", "One or more required fields are empty", ErrorCategory.Validation);


}
