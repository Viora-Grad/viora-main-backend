using Viora.Domain.Abstractions;

namespace Viora.Domain.Subscriptions;

public static class SubscriptionError
{
    public static readonly Error InvalidType = new Error("invalidSubscriptionType", "The provided subscription type is invalid.", ErrorCategory.Validation);
    public static readonly Error InvalidStatus = new Error("invalidSubscriptionStatus", "The provided subscription status is invalid.", ErrorCategory.Validation);
    public static readonly Error SubscriptionAlreadyActive = new Error("subscriptionAlreadyActive", "The subscription is already active.", ErrorCategory.Validation);
    public static readonly Error OrganizationNotSubscribed = new Error("organizationNotSubscribed", "The organization does not have a subscription.", ErrorCategory.Validation);
    public static readonly Error SubscriptionNotActivated = new Error("subscriptionNotActivated", "The subscription is not activated.", ErrorCategory.Validation);
    public static readonly Error LimitExceeded = new Error("limitExceeded", "The usage limit for the feature has been exceeded.", ErrorCategory.Validation);
}
