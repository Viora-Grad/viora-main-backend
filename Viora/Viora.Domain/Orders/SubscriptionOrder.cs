using Viora.Domain.Abstractions;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Plans;
using Viora.Domain.Shared;

namespace Viora.Domain.Orders;

public class SubscriptionOrder : Order
{
    public Guid PlanId { get; private set; }
    public SubscriptionOrderType SubscriptionOrderType { get; private set; }

    private SubscriptionOrder()
    {
        // Required by EF Core
    }

    private SubscriptionOrder(
        Guid id,
        Guid organizationId,
        Money totalPrice,
        DateTime createdDate,
        Guid planId,
        SubscriptionOrderType subscriptionOrderType,
        OrderStatus status)
        : base(id, organizationId, totalPrice, createdDate, status)
    {
        PlanId = planId;
        SubscriptionOrderType = subscriptionOrderType;
    }

    private SubscriptionOrder(
        Guid id,
        Guid organizationId,
        Guid subscriptionId,
        Money totalPrice,
        DateTime createdAt,
        Guid planId,
        SubscriptionOrderType subscriptionOrderType,
        OrderStatus status)
        : base(id, organizationId, subscriptionId, totalPrice, createdAt, status)
    {
        PlanId = planId;
        SubscriptionOrderType = subscriptionOrderType;
    }

    public static Result<SubscriptionOrder> CreateNewSubscriptionOrder(
            Guid organizationId,
            Plan plan,
            DateTime createdAt)
    {
        var newSubscriptionOrder = new SubscriptionOrder(
            Guid.NewGuid(),
            organizationId,
            plan.Price,
            createdAt,
            plan.Id,
            SubscriptionOrderType.NewSubscription,
            OrderStatus.Draft
        );
        return Result.Success(newSubscriptionOrder);
    }

    public static Result<SubscriptionOrder> CreateRenewSubscriptionOrder(
        Guid organizationId,
        Guid planId,
        Guid subscriptionId,
        Money totalPrice,
        DateTime createdAt)
    {
        var newSubscriptionOrder = new SubscriptionOrder(
            Guid.NewGuid(),
            organizationId,
            subscriptionId,
            totalPrice,
            createdAt,
            planId,
            SubscriptionOrderType.Renewal,
            OrderStatus.Draft
        );
        return Result.Success(newSubscriptionOrder);
    }

    // Carries the existing subscriptionId so a paid change order can resolve which
    // subscription to move (and derive the old plan id at provisioning time).
    public static Result<SubscriptionOrder> CreateChangeSubscriptionOrder(Guid organizationId, Guid subscriptionId, Plan newPlan, DateTime createdAt)
    {
        var changeSubscriptionOrder = new SubscriptionOrder(
            Guid.NewGuid(),
            organizationId,
            subscriptionId,
            newPlan.Price,
            createdAt,
            newPlan.Id,
            SubscriptionOrderType.ChangeSubscription,
            OrderStatus.Draft
        );
        return Result.Success(changeSubscriptionOrder);
    }
}
