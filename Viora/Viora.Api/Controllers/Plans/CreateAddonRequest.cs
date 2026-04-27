namespace Viora.Api.Controllers.Plans;

public class CreateAddonRequest
{
    public Guid SubscriptionId { get; set; }
    public Guid LimitedFeatureId { get; set; }
    public Guid LimitedFeatureAddonId { get; set; }
}
