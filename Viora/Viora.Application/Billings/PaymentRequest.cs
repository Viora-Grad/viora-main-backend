namespace Viora.Application.Billings;

public sealed record PaymentRequest
{
    public required string Amount { get; init; }            // string, e.g. "1.00"
    public required string Currency { get; init; }          // e.g. "EGP"
    public required string Order { get; init; }             // merchant order reference (your invoice id)
    public required string MerchantId { get; init; }        // "MID-..."
    public required string ServerWebhook { get; init; }     // dynamic webhook URL (per request)
    public required string MerchantRedirect { get; init; }  // post-payment browser redirect
    public required PaymentCustomer Customer { get; init; }
    public required DateTime ExpireAt { get; init; }
    public string Description { get; init; } = string.Empty;

    public string Type { get; init; } = "one-time";
    public string PaymentType { get; init; } = "credit";
    public int MaxFailureAttempts { get; init; } = 3;
    public bool ManualCapture { get; init; } = false;
    public bool Enable3DS { get; init; } = true;
    public bool FailureRedirect { get; init; } = false;
    public bool RetrieveSavedCard { get; init; } = true;
    public string SaveCard { get; init; } = "optional";
    public string Display { get; init; } = "en";
    public string AllowedMethods { get; init; } = "card,wallet";
    public string DefaultMethod { get; init; } = "card";
    public string InteractionSource { get; init; } = "ECOMMERCE";
    public string IframeBackgroundColor { get; init; } = "#FFFFFF";
    public string? RedirectMethod { get; init; }
    public string? BrandColor { get; init; }
    public string? Notes { get; init; }
    public object? MetaData { get; init; }
}

public sealed record PaymentCustomer(string Email, string Reference);
