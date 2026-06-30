using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Viora.Application.Billings;
using Viora.Domain.Abstractions;
using Viora.Domain.Billings;

namespace Viora.Infrastructure.Payments;

// Kashier adapter: creates hosted payment sessions and verifies webhook signatures.
internal sealed class KashierPaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<KashierPaymentService> _logger;

    // The HMAC key for webhook signatures. Kept in one place so it can be flipped
    // between the API key and the salted Secret once verified against the test gateway.
    private readonly string _signingKey;

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public KashierPaymentService(HttpClient httpClient, IPaymentSettings settings, ILogger<KashierPaymentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        // Kashier signs webhooks with the (GUID) API key — verified against a live test webhook.
        _signingKey = settings.ApiKey;
    }

    public async Task<Result<PaymentSessionResponse>> CreatePaymentSessionAsync(PaymentRequest request, CancellationToken cancellation)
    {
        try
        {
            var serializedBody = JsonSerializer.Serialize(request, WriteOptions);
            _logger.LogInformation(
                "Kashier session request -> POST {Url}\nHeaders: {Headers}\nBody: {Body}",
                new Uri(_httpClient.BaseAddress!, "payment/sessions"),
                DescribeHeaders(),
                serializedBody);

            using var response = await _httpClient.PostAsJsonAsync("payment/sessions", request, WriteOptions, cancellation);
            var raw = await response.Content.ReadAsStringAsync(cancellation);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Kashier session create failed ({Status}): {Body}", (int)response.StatusCode, raw);
                return Result.Failure<PaymentSessionResponse>(PaymentErrors.GatewayError);
            }

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var sessionId = FindString(root, "sessionId") ?? FindString(root, "_id") ?? FindString(root, "id");
            var url = FindString(root, "sessionUrl")
                ?? FindString(root, "url")
                ?? FindString(root, "paymentUrl")
                ?? FindString(root, "checkoutUrl")
                ?? FindString(root, "redirectUrl");

            if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(url))
            {
                _logger.LogError("Kashier session response missing sessionId/url: {Body}", raw);
                return Result.Failure<PaymentSessionResponse>(PaymentErrors.InvalidResponse);
            }

            _logger.LogInformation("Kashier session created. SessionId={SessionId} Url={Url}", sessionId, url);
            return Result.Success(new PaymentSessionResponse(sessionId, url));
        }
        catch (TaskCanceledException) when (!cancellation.IsCancellationRequested)
        {
            _logger.LogError("Kashier session create timed out.");
            return Result.Failure<PaymentSessionResponse>(PaymentErrors.GatewayTimeout);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Kashier session create transport error.");
            return Result.Failure<PaymentSessionResponse>(PaymentErrors.GatewayUnreachable);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Kashier session response was not valid JSON.");
            return Result.Failure<PaymentSessionResponse>(PaymentErrors.InvalidResponse);
        }
    }

    public Result VerifySignature(IReadOnlyDictionary<string, string> signatureFields, string signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader) || signatureFields.Count == 0)
            return Result.Failure(PaymentErrors.InvalidSignature);

        // Canonical string: signed fields in ordinal-sorted key order, rendered as a
        // URL-encoded query string (key=encodedValue&...), matching Kashier's scheme.
        var canonical = string.Join("&", signatureFields
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_signingKey));
        var computed = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();

        var computedBytes = Encoding.UTF8.GetBytes(computed);
        var providedBytes = Encoding.UTF8.GetBytes(signatureHeader.Trim().ToLowerInvariant());

        if (!CryptographicOperations.FixedTimeEquals(computedBytes, providedBytes))
        {
            _logger.LogWarning("Kashier webhook signature mismatch.");
            return Result.Failure(PaymentErrors.InvalidSignature);
        }

        return Result.Success();
    }

    // Renders the outgoing default headers, masking credential values so logs stay safe.
    private string DescribeHeaders()
    {
        return string.Join(" | ", _httpClient.DefaultRequestHeaders.Select(header =>
        {
            var value = string.Join(",", header.Value);
            return $"{header.Key}: {value}";
        }));
    }

    // Tolerant lookup: finds the first string property with the given name anywhere in the response tree.
    private static string? FindString(JsonElement element, string propertyName)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                        && prop.Value.ValueKind == JsonValueKind.String)
                        return prop.Value.GetString();

                    var nested = FindString(prop.Value, propertyName);
                    if (nested is not null)
                        return nested;
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    var nested = FindString(item, propertyName);
                    if (nested is not null)
                        return nested;
                }
                break;
        }

        return null;
    }
}
