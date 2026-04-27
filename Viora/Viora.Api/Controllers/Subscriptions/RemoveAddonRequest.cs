namespace Viora.Api.Controllers.Subscriptions;

public class RemoveAddonRequest
{
    public Guid SubscriptionId { get; set; }
    public Guid SubscriptionAddonId { get; set; }
}
