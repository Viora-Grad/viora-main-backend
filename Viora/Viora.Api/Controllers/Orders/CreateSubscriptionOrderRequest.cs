namespace Viora.Api.Controllers.Orders;

public class CreateSubscriptionOrderRequest
{
    public Guid OrganizationId { get; set; }
    public Guid PlanId { get; set; }
}
