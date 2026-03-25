namespace Viora.Domain.Abstractions;

public record Error(string Name, string Description, ErrorCategory Category)
{
    public static Error NoError => new(string.Empty, string.Empty, ErrorCategory.None);
    public static Error NullValue => new("Error.NullValue", "Null value was returned", ErrorCategory.NotFound);
}

public enum ErrorCategory
{
    None,
    Unknown,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Internal,
    Violation // for business rule violations (e.g., trying to delete a plan that has active subscriptions , should it be broken down to more specific categories?)
}