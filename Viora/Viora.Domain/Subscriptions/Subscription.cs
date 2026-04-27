using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Domain.Subscriptions;

public class Subscription : Entity
{
    public Guid PlanId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public SubscriptionStatus Status { get; private set; }


    public DateTime SubscriptionsStartTime { get; private set; }
    public DateTime SubscriptionsEndTime { get; private set; }

    private readonly List<SubscriptionAddon> _addons = new();
    public IReadOnlyCollection<SubscriptionAddon> Addons => _addons;

    private Subscription(Guid Id, Guid planId, Guid organizationId, SubscriptionStatus status, DateTime subscriptionsStartTime, DateTime subscriptionsEndTime) : base(Id)
    {
        PlanId = planId;
        OrganizationId = organizationId;
        Status = status;
        SubscriptionsStartTime = subscriptionsStartTime;
        SubscriptionsEndTime = subscriptionsEndTime;
    }


    public static Result<Subscription> Create(Guid planId, Guid organizationId, DateTime periodStart, DateTime periodEnd)
    {
        var subscriptionStatus = SubscriptionStatus.Active;
        var newSubscription = new Subscription(Guid.NewGuid(), planId, organizationId, subscriptionStatus, periodStart, periodEnd);
        /*newSubscription.RaiseDomainEvent(
            new SubscriptionCreatedDomainEvent(
                planId,
                organizationId
            )
        );*/
        return Result.Success(newSubscription);
    }

    public Result<Subscription> Renew(DateTime periodStart, DateTime periodEnd)
    {
        Status = SubscriptionStatus.Expired;
        if (SubscriptionsEndTime > periodStart)
        {
            periodEnd = periodEnd.Add(SubscriptionsEndTime.Subtract(periodStart));
            SubscriptionsEndTime = periodStart;
        }

        var renwedSubscription = new Subscription(Guid.NewGuid(), PlanId, OrganizationId, SubscriptionStatus.Active, periodStart, periodEnd);

        /*RaiseDomainEvent(
            new SubscriptionRenewedDomainEvent(
                Id,
                PlanId,
                OrganizationId
            )
        );*/
        return Result.Success(renwedSubscription);
    }

    public Result<Subscription> ChangePlan(Guid organizationId, Guid oldPlanId, Guid newplanId, DateTime startTime, DateTime endTime)
    {
        Status = SubscriptionStatus.Canceled;

        var newSubscription = new Subscription(Guid.NewGuid(), newplanId, organizationId, SubscriptionStatus.Active, startTime, endTime);

        /*RaiseDomainEvent(
            new SubscriptionPlanChangedDomainEvent(
                Id,
                oldPlanId,
                newplanId,
                organizationId,
                startTime,
                endTime
            )
        );*/
        return Result.Success(newSubscription);
    }


    public Result AddAddons(List<Guid> addonIds)
    {
        if (addonIds is null || !addonIds.Any())
            return Result.Failure(SubscriptionError.InvalidAddonList);
        var subscriptionAddon = SubscriptionAddon.CreateMany(addonIds, Id);
        _addons.AddRange(subscriptionAddon);
        return Result.Success();
    }

    public List<SubscriptionAddon> GetAddons()
    {
        return _addons;
    }
}
