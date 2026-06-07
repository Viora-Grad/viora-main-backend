using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.GetOrganizationSubscriptions;

public class SubscriptionResponse
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Status { get; set; }
    public DateTime SubscriptionStartTime { get; set; }
    public DateTime SubscriptionEndTime { get; set; }
    public List<SubscriptionAddonResponse>? SubscriptionAddonDtos { get; set; }

    private SubscriptionResponse(Guid id, Guid planId, Guid organizationId, string status, DateTime starTime, DateTime EndTime, List<SubscriptionAddonResponse> addon)
    {
        Id = id;
        PlanId = planId;
        OrganizationId = organizationId;
        Status = status;
        SubscriptionStartTime = starTime;
        SubscriptionEndTime = EndTime;
        SubscriptionAddonDtos = addon;
    }

    public static SubscriptionResponse MapToDto(Subscription subscription)
    {
        var dto = new SubscriptionResponse(
               subscription.Id,
               subscription.PlanId,
               subscription.OrganizationId,
               subscription.Status.Value,
               subscription.SubscriptionsStartTime,
               subscription.SubscriptionsEndTime,
               SubscriptionAddonResponse.MapToDto(subscription.GetAddons())
               );

        return dto;
    }
}
