using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.GetOrganizationSubscriptions;

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Status { get; set; }
    public DateTime SubscriptionStartTime { get; set; }
    public DateTime SubscriptionEndTime { get; set; }
    public List<SubscriptionAddonDto>? SubscriptionAddonDtos { get; set; }

    private SubscriptionDto(Guid id, Guid planId, Guid organizationId, string status, DateTime starTime, DateTime EndTime, List<SubscriptionAddonDto> addon)
    {
        Id = id;
        PlanId = planId;
        OrganizationId = organizationId;
        Status = status;
        SubscriptionStartTime = starTime;
        SubscriptionEndTime = EndTime;
        SubscriptionAddonDtos = addon;
    }

    public static SubscriptionDto MapToDto(Subscription subscription)
    {
        var dto = new SubscriptionDto(
               subscription.Id,
               subscription.PlanId,
               subscription.OrganizationId,
               subscription.Status.Value,
               subscription.SubscriptionsStartTime,
               subscription.SubscriptionsEndTime,
               SubscriptionAddonDto.MapToDto(subscription.GetAddons())
               );

        return dto;
    }
}
