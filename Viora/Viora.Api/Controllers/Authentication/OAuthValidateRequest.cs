namespace Viora.Api.Controllers.Authentication;

public sealed record OAuthValidateRequest(string? Token, string? Code, string? RedirectUri)
{
    public bool IsValid => (Token is not null ^ Code is not null) || (Code is not null && RedirectUri is not null);
    public bool IsToken => Token is not null;
    public bool IsCode => Code is not null;
}
