namespace Viora.Api.Controllers.Authentication;

public sealed record OrganizationLoginRequest(string Username, string Password)
{
}
