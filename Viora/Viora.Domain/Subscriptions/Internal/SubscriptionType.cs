using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions.Internal;

public sealed class SubscriptionType
{
    public static readonly SubscriptionType monthly = new(1, "Monthly");
    public static readonly SubscriptionType annually = new(2, "Annually");
    public static readonly SubscriptionType semiAnnually = new(3, "Semi-Annually");

    private SubscriptionType(int id, string name)
    {
        this.Id = id;
        this.Name = name;
    }
    public int Id { get; private set; }
    public string Name { get; private set; }


    public static Result<SubscriptionType> CheckSubscriptionType(string subscriptionType)
    {
        if (subscriptionType == monthly.Name)
            return Result.Success(monthly);
        else if (subscriptionType == annually.Name)
            return Result.Success(annually);
        else if (subscriptionType == semiAnnually.Name)
            return Result.Success(semiAnnually);
        else
            return Result.Failure<SubscriptionType>(SubscriptionError.InvalidType);
    }
}
