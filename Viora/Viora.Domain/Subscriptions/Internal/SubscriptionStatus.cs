using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions.Internal;

public record SubscriptionStatus(string Value)
{
    public static readonly SubscriptionStatus Active = new("Active");
    public static readonly SubscriptionStatus Canceled = new("Canceled");
    public static readonly SubscriptionStatus Expired = new("Expired");
    public static Result<SubscriptionStatus> CheckSubscriptionStatus(string subscriptionStatus)
    {
        if (subscriptionStatus == Active.Value)
            return Result.Success(Active);
        else if (subscriptionStatus == Canceled.Value)
            return Result.Success(Canceled);
        else if (subscriptionStatus == Expired.Value)
            return Result.Success(Expired);
        else
            return Result.Failure<SubscriptionStatus>(SubscriptionError.InvalidStatus);
    }

}
