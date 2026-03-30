using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions;

public static class SubscriptionError
{
    public static readonly Error InvalidType = new Error("invalidSubscriptionType", "The provided subscription type is invalid.", ErrorCategory.Validation);
    public static readonly Error InvalidStatus = new Error("invalidSubscriptionStatus", "The provided subscription status is invalid.", ErrorCategory.Validation);
    public static readonly Error SubscriptionAlreadyActive = new Error("subscriptionAlreadyActive", "The subscription is already active.", ErrorCategory.Validation);
}
