using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.ConnectMetaPage;

// Connects a Facebook Page to the caller's organization via the OAuth token-exchange flow:
//   AuthCode = a short-lived user token (fb_exchange_token) obtained on the client via Facebook Login.
//   PageId   = the id of the Page to connect.
// The handler exchanges AuthCode for a long-lived user token, resolves the Page's own access token from
// GET /me/accounts, then stores that (encrypted) as the organization's Page credential.
public sealed record ConnectMetaPageCommand(string AuthCode, string PageId) : ICommand;
