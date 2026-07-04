namespace Viora.Api.Controllers.Wallets;

public sealed record RechargeRequest(decimal Amount);

public sealed record CheckoutRequest(
    decimal Amount,
    string Currency,
    string RecipientName,
    string RecipientBank,
    string RecipientNumber);
