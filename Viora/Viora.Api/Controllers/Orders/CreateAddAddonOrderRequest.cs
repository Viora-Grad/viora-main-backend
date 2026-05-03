namespace Viora.Api.Controllers.Orders;

public class CreateAddAddonOrderRequest
{
    public Guid SubscriptionId { get; set; }
    public Guid OrganizationId { get; set; }
    public List<Guid> Addons { get; set; } = new List<Guid>();
}

