using Viora.Domain.Abstractions;

namespace Viora.Application.Billings;

public interface IPaymentService
{
    public Task<Result<PaymentResponse>> CreatePaymentSessionAsync(PaymentRequest requst, CancellationToken cancellation);
    public Task<Result> VerifySignature(string hash);
}
