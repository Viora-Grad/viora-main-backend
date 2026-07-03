namespace Viora.Application.Billings;

// Branch checkout: a Kashier single bank transfer. Only bank transfers are supported (method is fixed
// to "bank" by the adapter).
public sealed record PayoutRequest
{
    public required decimal Amount { get; init; }
    public required string RecipientName { get; init; }
    public required string RecipientBank { get; init; }      // bank code/name, e.g. "CIB"
    public required string RecipientNumber { get; init; }    // recipient account / wallet number
    public required string MerchantTransferId { get; init; } // our correlation id
}

// The reference used to reconcile the payout (our merchant transfer id).
public sealed record PayoutResponse(string Reference);
