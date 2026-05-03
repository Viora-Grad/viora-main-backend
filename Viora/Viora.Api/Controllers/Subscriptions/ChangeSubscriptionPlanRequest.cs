namespace Viora.Api.Controllers.Subscriptions;

public class ChangeSubscriptionPlanRequest
{
    public Guid SubscriptionId { get; set; }
    public Guid NewPlanId { get; set; }

}
