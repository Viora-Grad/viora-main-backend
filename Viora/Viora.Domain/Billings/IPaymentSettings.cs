namespace Viora.Domain.Billings;

public interface IPaymentSettings
{
    public string MerchentId { get; set; }
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
    public string Secret { get; set; }
    public string PublicBaseUrl { get; set; }
    public string ClientBaseUrl { get; set; }
}
