namespace Viora.Application.Billings;

// Kashier payment-session create response: the session id and the hosted-payment-page URL
// we hand back to the customer. Distinct from PaymentResponse (the webhook payload).
public sealed record PaymentSessionResponse(string SessionId, string SessionUrl);
