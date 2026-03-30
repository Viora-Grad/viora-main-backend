using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Domain.Subscriptions;

public class Subscription : Entity
{
    public Guid PlanId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public SubscriptionStatus Stauts { get; private set; }
    public SubscriptionType Type { get; private set; }

    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }

    private Subscription(Guid Id, Guid planId, Guid organizationId, SubscriptionType subscriptionType, SubscriptionStatus stauts, DateTime periodStart, DateTime periodEnd) : base(Id)
    {
        this.PlanId = planId;
        this.OrganizationId = organizationId;
        this.Type = subscriptionType;
        this.Stauts = stauts;
        this.PeriodStart = periodStart;
        this.PeriodEnd = periodEnd;
    }


    public static Result<Subscription> Create(Guid planId, Guid organizationId, string subscriptionType, DateTime periodStart)
    {
        var subscriptionStatus = SubscriptionStatus.Active;
        var subscriptionTypeResult = SubscriptionType.CheckSubscriptionType(subscriptionType);
        if (subscriptionTypeResult.IsFailure)
        {
            return Result.Failure<Subscription>(subscriptionTypeResult.Error);
        }
        var type = subscriptionTypeResult.Value;
        var SubscriptionEndingResult = CalculateSubscriptionEndingPeriod(type, periodStart);
        if (SubscriptionEndingResult.IsFailure)
        {
            return Result.Failure<Subscription>(SubscriptionEndingResult.Error);
        }
        return Result.Success(
            new Subscription(Guid.NewGuid(), planId, organizationId, type, SubscriptionStatus.Active, periodStart, SubscriptionEndingResult.Value));

    }


    public static Result<DateTime> CalculateSubscriptionEndingPeriod(SubscriptionType subscriptionType, DateTime periodStart)
    {
        if (subscriptionType == SubscriptionType.monthly)
        {
            return Result.Success(periodStart.AddMonths(1));
        }
        else if (subscriptionType == SubscriptionType.annually)
        {
            return Result.Success(periodStart.AddYears(1));
        }
        else if (subscriptionType == SubscriptionType.semiAnnually)
        {
            return Result.Success(periodStart.AddMonths(6));
        }
        else
            return Result.Failure<DateTime>(SubscriptionError.InvalidType);
    }
}
