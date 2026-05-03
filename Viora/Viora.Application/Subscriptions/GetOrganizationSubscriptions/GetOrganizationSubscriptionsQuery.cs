using Viora.Application.Abstractions.Messaging;


namespace Viora.Application.Subscriptions.GetOrganizationSubscriptions;

public sealed record GetOrganizationSubscriptionsQuery(
    Guid OrganizationId) : IQuery<List<SubscriptionDto>>;

