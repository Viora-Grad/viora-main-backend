using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions.Events;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Domain.Subscriptions;

public class Subscription : Entity
{
    public Guid PlanId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public SubscriptionStatus Status { get; private set; }

    public DateTime SubscriptionsStartTime { get; private set; }
    public DateTime SubscriptionsEndTime { get; private set; }

    private Subscription(Guid Id, Guid planId, Guid organizationId, SubscriptionStatus stauts, DateTime startTime, DateTime endTime) : base(Id)
    {
        PlanId = planId;
        OrganizationId = organizationId;
        Status = stauts;
        SubscriptionsStartTime = startTime;
        SubscriptionsEndTime = endTime;
    }


    public static Result<Subscription> Create(Guid planId, Guid organizationId, DateTime periodStart, DateTime periodEnd)
    {
        var subscriptionStatus = SubscriptionStatus.Active;
        var newSubscription = new Subscription(Guid.NewGuid(), planId, organizationId, subscriptionStatus, periodStart, periodEnd);
        newSubscription.RaiseDomainEvent(
            new SubscriptionCreatedDomainEvent(
                newSubscription.Id,
                planId,
                organizationId,
                periodStart,
                periodEnd
            )
        );
        return Result.Success(newSubscription);
    }
    public Result Renew(DateTime periodStart, DateTime periodEnd)
    {
        if (Status == SubscriptionStatus.Active)
        {
            return Result.Failure(SubscriptionError.SubscriptionAlreadyActive);
        }
        Status = SubscriptionStatus.Active;
        SubscriptionsStartTime = periodStart;
        SubscriptionsEndTime = periodEnd;
        return Result.Success();
    }

}
