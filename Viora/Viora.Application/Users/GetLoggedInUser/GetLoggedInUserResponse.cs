namespace Viora.Application.Users.GetLoggedInUser;

public sealed record GetLoggedInUserResponse
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public string Gender { get; init; }
}