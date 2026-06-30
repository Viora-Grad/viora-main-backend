using System.Text.Json;
using Viora.Application.Billings;
using Viora.Domain.Abstractions;

namespace Viora.Application.Payments.Webhooks;

internal sealed record ParsedWebhook(PaymentResponse Payload, IReadOnlyDictionary<string, string> SignatureFields);

internal static class KashierWebhookParser
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    // Deserializes the webhook body and rebuilds the exact field set Kashier signed,
    // reading raw values out of the "data" object by the names listed in signatureKeys.
    public static Result<ParsedWebhook> Parse(string rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
            return Result.Failure<ParsedWebhook>(PaymentErrors.InvalidResponse);

        try
        {
            var payload = JsonSerializer.Deserialize<PaymentResponse>(rawBody, Options);
            if (payload?.Data is null)
                return Result.Failure<ParsedWebhook>(PaymentErrors.InvalidResponse);

            var fields = new Dictionary<string, string>(StringComparer.Ordinal);

            using var doc = JsonDocument.Parse(rawBody);
            if (doc.RootElement.TryGetProperty("data", out var data)
                && data.ValueKind == JsonValueKind.Object
                && payload.Data.SignatureKeys is not null)
            {
                foreach (var key in payload.Data.SignatureKeys)
                {
                    if (data.TryGetProperty(key, out var value))
                        fields[key] = value.ValueKind == JsonValueKind.String
                            ? value.GetString() ?? string.Empty
                            : value.GetRawText();
                }
            }

            return Result.Success(new ParsedWebhook(payload, fields));
        }
        catch (JsonException)
        {
            return Result.Failure<ParsedWebhook>(PaymentErrors.InvalidResponse);
        }
    }
}
