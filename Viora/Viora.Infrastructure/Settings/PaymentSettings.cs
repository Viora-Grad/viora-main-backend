using Viora.Domain.Billings;

namespace Viora.Infrastructure.Settings;

public class PaymentSettings : IPaymentSettings
{
    public string MerchentId { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
    public string Secret { get; set; } = default!;
    public string PublicBaseUrl { get; set; } = default!;
    public string ClientBaseUrl { get; set; } = default!;
    public string TransferUrl { get; set; } = default!;
}
