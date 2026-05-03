namespace Viora.Api.Controllers.Orders;

public class ChangeSubscriptionOrderRequest
{
    public Guid SubscriptionId { get; set; }
    public Guid NewPlanId { get; set; }
}
