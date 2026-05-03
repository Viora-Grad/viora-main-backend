using Viora.Domain.Abstractions;

namespace Viora.Domain.Orders.Internal;

public record SubscriptionOrderType(string Value)
{
    public static readonly SubscriptionOrderType NewSubscription = new SubscriptionOrderType("NewSubscription");
    public static readonly SubscriptionOrderType ChangeSubscription = new SubscriptionOrderType("ChangeSubscription");
    public static readonly SubscriptionOrderType Renewal = new SubscriptionOrderType("Renewal");

    public static Result<SubscriptionOrderType> FromValue(string value)
    {
        return value switch
        {
            "NewSubscription" => Result.Success(NewSubscription),
            "ChangeSubscription" => Result.Success(ChangeSubscription),
            "Renewal" => Result.Success(Renewal),
            _ => Result.Failure<SubscriptionOrderType>(OrderError.InvalidSubscriptionOrderType)
        };
    }
}
