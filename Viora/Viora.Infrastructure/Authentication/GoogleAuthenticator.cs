using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.Extensions.Configuration;
using Viora.Application.Abstractions.Authentication;

namespace Viora.Infrastructure.Authentication;

internal class GoogleAuthenticator(IConfiguration config) : IGoogleAuthenticator
{
    public string ClientId => config["Google:ClientId"]!;
    public string ClientSecret => config["Google:ClientSecret"]!;

    public async Task<string> GetGoogleIdTokenAsync(string code, string redirectUri, CancellationToken ct = default)
    {
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret
            },
        });
        try
        {
            TokenResponse token = await flow.ExchangeCodeForTokenAsync(
                userId: "user",
                code: code,
                redirectUri: redirectUri,
                taskCancellationToken: ct);
            return token.IdToken;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
