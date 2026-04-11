using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Domain.Subscriptions;

public class Subscription : Entity
{
    public Guid PlanId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public SubscriptionStatus Stauts { get; private set; }

    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }

    private Subscription(Guid Id, Guid planId, Guid organizationId, SubscriptionStatus stauts, DateTime periodStart, DateTime periodEnd) : base(Id)
    {
        this.PlanId = planId;
        this.OrganizationId = organizationId;
        this.Stauts = stauts;
        this.PeriodStart = periodStart;
        this.PeriodEnd = periodEnd;
    }


    public static Result<Guid> Create(Guid planId, Guid organizationId, string subscriptionType, DateTime periodStart, DateTime periodEnd)
    {
        var subscriptionStatus = SubscriptionStatus.Active;
        var newSubscription = new Subscription(Guid.NewGuid(), planId, organizationId, subscriptionStatus, periodStart, periodEnd);
        return Result.Success(newSubscription.Id);
    }
    public Result Renew(DateTime periodStart, string newsubscriptionType)
    {
        if (Stauts == SubscriptionStatus.Active)
        {
            return Result.Failure(SubscriptionError.SubscriptionAlreadyActive);
        }
        var SubscriptionTypeResult = PlanPeriod.CheckSubscriptionType(newsubscriptionType);
        if (SubscriptionTypeResult.IsFailure)
        {
            return Result.Failure(SubscriptionTypeResult.Error);
        }
        this.PeriodEnd = CalculateSubscriptionEndingPeriod(newsubscriptionType, periodStart).Value;
        return Result.Success();
    }

}
