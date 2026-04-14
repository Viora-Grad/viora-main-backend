using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Domain.Subscriptions;

public class Subscription : Entity
{
    public Guid PlanId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public SubscriptionStatus Stauts { get; private set; }

    public DateTime SubscriptionsStartTime { get; private set; }
    public DateTime SubscriptionEndTime { get; private set; }

    private Subscription(Guid Id, Guid planId, Guid organizationId, SubscriptionStatus stauts, DateTime startTime, DateTime endTime) : base(Id)
    {
        PlanId = planId;
        OrganizationId = organizationId;
        Stauts = stauts;
        SubscriptionsStartTime = startTime;
        SubscriptionEndTime = endTime;
    }


    public static Result<Subscription> Create(Guid planId, Guid organizationId, DateTime periodStart, DateTime periodEnd)
    {
        var subscriptionStatus = SubscriptionStatus.Active;
        var newSubscription = new Subscription(Guid.NewGuid(), planId, organizationId, subscriptionStatus, periodStart, periodEnd);
        return Result.Success(newSubscription);
    }
    public Result Renew(DateTime periodStart, DateTime periodEnd)
    {
        if (Stauts == SubscriptionStatus.Active)
        {
            return Result.Failure(SubscriptionError.SubscriptionAlreadyActive);
        }
        Stauts = SubscriptionStatus.Active;
        SubscriptionsStartTime = periodStart;
        SubscriptionEndTime = periodEnd;
        return Result.Success();
    }

}
