namespace Viora.Application.Abstractions.Authentication;

/// <summary>
/// Represents the context of the currently authenticated user.
/// This interface provides access to the user's unique identifier and their type (e.g., Owner, Customer).
/// It is typically used in application services and controllers to retrieve information about the current user 
/// without needing to directly access the authentication mechanism or token details.
/// Implementations of this interface should ensure that the UserId and UserType properties are correctly populated based on the authenticated user's information.
/// </summary>
/// <remarks>
/// Not sure how this is going to be implemented yet, but it will likely involve roles and claims in the future. It is simple for now
/// </remarks>
public interface IUserContext
{
    Guid UserId { get; }
}

