using System.Text.Json;
using System.Text.Json.Serialization;

namespace Viora.Application.Billings;

public sealed record PaymentResponse(
    string Event,
    PaymentData Data,
    string Hash);

public sealed record PaymentData(
    string MerchantOrderId,
    string KashierOrderId,
    string OrderReference,
    string TransactionId,
    string Status,
    string Method,
    DateTime CreationDate,
    decimal Amount,
    string Currency,
    SettlementInfo SettlementInfo,
    PaymentCard Card,
    string TransactionResponseCode,
    LocalizedMessage TransactionResponseMessage,
    string Channel,
    MerchantDetails MerchantDetails,

    IReadOnlyList<string> SignatureKeys,

    JsonElement? MetaData,
    JsonElement? SourceOfFunds,
    JsonElement? InstallmentPlan);

public sealed record SettlementInfo(
    string Vat,
    decimal SellingRate,
    decimal SellingFlat,
    string TotalSellingRate,
    string TotalSellingFees,
    string SettledAmount);

public sealed record PaymentCard(
    CardInfo CardInfo,
    CardMerchant Merchant,
    decimal Amount,
    string Currency);

public sealed record CardInfo(
    string CardHolderName,
    string CardBrand,
    string MaskedCard);

public sealed record CardMerchant(
    [property: JsonPropertyName("merchantRedirectURL")] string MerchantRedirectUrl);

public sealed record LocalizedMessage(
    [property: JsonPropertyName("en")] string En,
    [property: JsonPropertyName("ar")] string Ar);

public sealed record MerchantDetails(string BusinessEmail);
