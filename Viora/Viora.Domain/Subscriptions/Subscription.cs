using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Domain.Subscriptions;

public class Subscription : Entity
{
    public Guid PlanId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public SubscriptionStatus Stauts { get; private set; }

    public TimeSpan PeriodStart { get; private set; }
    public TimeSpan PeriodEnd { get; private set; }

    private Subscription(Guid Id, Guid planId, Guid organizationId, SubscriptionStatus stauts, TimeSpan periodStart, TimeSpan periodEnd) : base(Id)
    {
        this.PlanId = planId;
        this.OrganizationId = organizationId;
        this.Stauts = stauts;
        this.PeriodStart = periodStart;
        this.PeriodEnd = periodEnd;
    }

}
