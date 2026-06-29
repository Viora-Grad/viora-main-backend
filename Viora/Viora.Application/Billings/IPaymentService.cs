using Viora.Domain.Abstractions;

namespace Viora.Application.Billings;

public interface IPaymentService
{
    // Creates a hosted payment session at the gateway and returns the session id + hosted-pay URL.
    Task<Result<PaymentSessionResponse>> CreatePaymentSessionAsync(PaymentRequest request, CancellationToken cancellation);

    // Verifies a webhook's x-kashier-signature header against the signed data fields (HMAC-SHA256, CPU-only).
    Result VerifySignature(IReadOnlyDictionary<string, string> signatureFields, string signatureHeader);
}
