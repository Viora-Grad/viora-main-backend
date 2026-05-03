namespace Viora.Api.Controllers.Subscriptions;

public class CreateSubscriptionRequest
{
    public Guid PlanId { get; set; }
    public Guid OrganizationID { get; set; }
}
