using Viora.Domain.Abstractions;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Plans;

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
        double totalPrice,
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
        double totalPrice,
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
            OrderStatus.Pending
        );
        // Raise the orderPaidEvent 
        return Result.Success(newSubscriptionOrder);
    }

    public static Result<SubscriptionOrder> CreateRenewSubscriptionOrder(
        Guid organizationId,
        Guid planId,
        Guid subscriptionId,
        double totalPrice,
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
            OrderStatus.Pending
        );
        // Raise the orderPaidEvent 
        return Result.Success(newSubscriptionOrder);
    }

    public static Result<SubscriptionOrder> CreateChangeSubscriptionOrder(Guid organizationId, Plan newPlan, DateTime createdAt)
    {
        var changeSubscriptionOrder = new SubscriptionOrder(
            Guid.NewGuid(),
            organizationId,
            newPlan.Price,
            createdAt,
            newPlan.Id,
            SubscriptionOrderType.ChangeSubscription,
            OrderStatus.Pending
        );
        // Raise the orderPaidEvent 
        return Result.Success(changeSubscriptionOrder);
    }
}
