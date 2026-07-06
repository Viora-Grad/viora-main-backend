using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Viora.Application.Marketing.Abstractions;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Infrastructure.Marketing;

// Facebook Pages Graph API adapter (typed HttpClient). Creates LIVE posts in one call (published=true). The
// per-tenant Page token is sent as an "Authorization: Bearer {token}" header, set per request.
//   Text/link:  POST {version}/{page-id}/feed    (message, link?, published=true)          -> { id }
//   Photo:      POST {version}/{page-id}/photos  (caption, source=binary, published=true)  -> { id, post_id }
internal sealed class MetaGraphService : IMetaGraphService
{
    private readonly HttpClient _httpClient;
    private readonly IMetaSettings _settings;
    private readonly ILogger<MetaGraphService> _logger;

    public MetaGraphService(HttpClient httpClient, IMetaSettings settings, ILogger<MetaGraphService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task<Result<MetaPostResult>> CreatePostAsync(
        string pageId, string accessToken, MetaPostPayload payload, CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string>
        {
            ["message"] = payload.Message,
            ["published"] = "true",
        };
        if (!string.IsNullOrWhiteSpace(payload.Link))
            form["link"] = payload.Link!;

        var path = $"{_settings.GraphApiVersion}/{pageId}/feed";

        return await SendAsync(path, new FormUrlEncodedContent(form), accessToken, Describe(form), cancellationToken, root =>
        {
            var id = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            return string.IsNullOrWhiteSpace(id)
                ? Result.Failure<MetaPostResult>(MarketingErrors.MetaGraphFailed)
                : Result.Success(new MetaPostResult(id!));
        });
    }

    public async Task<Result<MetaPostResult>> CreatePhotoPostAsync(
        string pageId, string accessToken, string caption, byte[] imageBytes, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var content = new MultipartFormDataContent
        {
            { new StringContent(caption), "caption" },
            { new StringContent("true"), "published" },
            { imageContent, "source", fileName },
        };

        var path = $"{_settings.GraphApiVersion}/{pageId}/photos";
        var bodyForLog = $"multipart -> caption='{caption}', published=true, source=<{imageBytes.Length} bytes, {contentType}, filename='{fileName}'>";

        return await SendAsync(path, content, accessToken, bodyForLog, cancellationToken, root =>
        {
            // A photo post returns the photo `id` and the story `post_id`; prefer the story post id.
            var postId = root.TryGetProperty("post_id", out var p) ? p.GetString() : null;
            var id = postId ?? (root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null);
            return string.IsNullOrWhiteSpace(id)
                ? Result.Failure<MetaPostResult>(MarketingErrors.MetaGraphFailed)
                : Result.Success(new MetaPostResult(id!));
        });
    }

    public async Task<Result<string>> ExchangeForLongLivedUserTokenAsync(
        string shortLivedUserToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.AppId) || string.IsNullOrWhiteSpace(_settings.AppSecret))
        {
            _logger.LogError("Meta App credentials (AppId/AppSecret) are not configured; cannot exchange token.");
            return Result.Failure<string>(MarketingErrors.TokenExchangeFailed);
        }

        var query =
            $"grant_type=fb_exchange_token" +
            $"&client_id={Uri.EscapeDataString(_settings.AppId)}" +
            $"&client_secret={Uri.EscapeDataString(_settings.AppSecret)}" +
            $"&fb_exchange_token={Uri.EscapeDataString(shortLivedUserToken)}";
        var path = $"{_settings.GraphApiVersion}/oauth/access_token?{query}";

        var result = await GetJsonAsync(path, "GET oauth/access_token (fb_exchange_token)", cancellationToken);
        if (result.IsFailure)
            return Result.Failure<string>(MarketingErrors.TokenExchangeFailed);

        var token = result.Value.TryGetProperty("access_token", out var el) ? el.GetString() : null;
        return string.IsNullOrWhiteSpace(token)
            ? Result.Failure<string>(MarketingErrors.TokenExchangeFailed)
            : Result.Success(token!);
    }

    public async Task<Result<string>> GetPageAccessTokenAsync(
        string userAccessToken, string pageId, CancellationToken cancellationToken)
    {
        // Start at /me/accounts; follow paging.next (absolute URLs) until the page is found or the list ends.
        var next = $"{_settings.GraphApiVersion}/me/accounts?limit=100&access_token={Uri.EscapeDataString(userAccessToken)}";

        for (var page = 0; page < MaxAccountPages && next is not null; page++)
        {
            var result = await GetJsonAsync(next, "GET me/accounts", cancellationToken);
            if (result.IsFailure)
                return Result.Failure<string>(MarketingErrors.MetaGraphFailed);

            var root = result.Value;
            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                foreach (var account in data.EnumerateArray())
                {
                    var id = account.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                    if (!string.Equals(id, pageId, StringComparison.Ordinal))
                        continue;

                    var token = account.TryGetProperty("access_token", out var tokEl) ? tokEl.GetString() : null;
                    return string.IsNullOrWhiteSpace(token)
                        ? Result.Failure<string>(MarketingErrors.PageNotInAccounts)
                        : Result.Success(token!);
                }
            }

            next = root.TryGetProperty("paging", out var paging) && paging.TryGetProperty("next", out var n)
                ? n.GetString()
                : null;
        }

        _logger.LogWarning("Page id {PageId} was not found among the user's managed pages.", pageId);
        return Result.Failure<string>(MarketingErrors.PageNotInAccounts);
    }

    private const int MaxAccountPages = 20;

    // Shared GET + JSON parse. `pathOrUrl` may be a relative path or an absolute paging URL. Never logs raw
    // tokens: the query (which carries access_token) is stripped from the logged URL.
    private async Task<Result<JsonElement>> GetJsonAsync(string pathOrUrl, string label, CancellationToken cancellationToken)
    {
        var requestUri = pathOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? new Uri(pathOrUrl)
            : _httpClient.BaseAddress is null ? new Uri(pathOrUrl, UriKind.Relative) : new Uri(_httpClient.BaseAddress, pathOrUrl);

        var safeUrl = requestUri.IsAbsoluteUri ? requestUri.GetLeftPart(UriPartial.Path) : pathOrUrl;
        _logger.LogInformation("Meta Graph request -> {Label}: {Url} (token in query, redacted)", label, safeUrl);

        try
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Meta Graph {Label} failed ({Status}) for {Url}\n  Response: {Response}",
                    label, (int)response.StatusCode, safeUrl, raw);
                return Result.Failure<JsonElement>(MarketingErrors.MetaGraphFailed);
            }

            using var doc = JsonDocument.Parse(raw);
            // Clone so the JsonElement stays valid after the JsonDocument is disposed.
            return Result.Success(doc.RootElement.Clone());
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Meta Graph {Label} timed out for {Url}.", label, safeUrl);
            return Result.Failure<JsonElement>(MarketingErrors.MetaGraphFailed);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Meta Graph {Label} transport error for {Url}.", label, safeUrl);
            return Result.Failure<JsonElement>(MarketingErrors.MetaGraphFailed);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Meta Graph {Label} response was not valid JSON for {Url}.", label, safeUrl);
            return Result.Failure<JsonElement>(MarketingErrors.MetaGraphFailed);
        }
    }

    // Shared POST + response handling. Logs the full outgoing request (absolute URL, body, masked token) so a
    // 4xx from Graph can be diagnosed, authenticates with a per-request Bearer header, and on 2xx runs onSuccess.
    private async Task<Result<MetaPostResult>> SendAsync(
        string path,
        HttpContent content,
        string accessToken,
        string bodyForLog,
        CancellationToken cancellationToken,
        Func<JsonElement, Result<MetaPostResult>> onSuccess)
    {
        var url = _httpClient.BaseAddress is null ? path : new Uri(_httpClient.BaseAddress, path).ToString();

        _logger.LogInformation(
            "Meta Graph request ->\n  POST {Url}\n  Authorization: Bearer {Token}\n  Body: {Body}",
            url, Mask(accessToken), bodyForLog);

        try
        {
            using (content)
            using (var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content })
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Meta Graph call failed ({Status}) for POST {Url}\n  Body sent: {Body}\n  Response: {Response}",
                        (int)response.StatusCode, url, bodyForLog, raw);
                    return Result.Failure<MetaPostResult>(MarketingErrors.MetaGraphFailed);
                }

                _logger.LogInformation("Meta Graph call succeeded ({Status}) for POST {Url}: {Response}",
                    (int)response.StatusCode, url, raw);

                using var doc = JsonDocument.Parse(raw);
                return onSuccess(doc.RootElement);
            }
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Meta Graph call timed out for POST {Url}.", url);
            return Result.Failure<MetaPostResult>(MarketingErrors.MetaGraphFailed);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Meta Graph transport error for POST {Url}.", url);
            return Result.Failure<MetaPostResult>(MarketingErrors.MetaGraphFailed);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Meta Graph response was not valid JSON for POST {Url}.", url);
            return Result.Failure<MetaPostResult>(MarketingErrors.MetaGraphFailed);
        }
    }

    // Renders form fields for logging (no token is ever in the body — it's an Authorization header).
    private static string Describe(Dictionary<string, string> form) =>
        string.Join("&", form.Select(kv => $"{kv.Key}={kv.Value}"));

    // Masks the token so logs confirm it's present/plausible without leaking it in full.
    private static string Mask(string token) =>
        string.IsNullOrEmpty(token) ? "(EMPTY!)"
        : token.Length <= 12 ? $"*** (len {token.Length})"
        : $"{token[..6]}…{token[^4..]} (len {token.Length})";
}
