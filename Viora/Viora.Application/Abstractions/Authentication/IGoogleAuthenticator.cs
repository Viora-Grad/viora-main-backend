namespace Viora.Application.Abstractions.Authentication;

public interface IGoogleAuthenticator
{
    public string ClientId { get; }
    public string ClientSecret { get; }
    public Task<string> GetGoogleIdTokenAsync(string code, string redirectUri, CancellationToken ct = default);
}
