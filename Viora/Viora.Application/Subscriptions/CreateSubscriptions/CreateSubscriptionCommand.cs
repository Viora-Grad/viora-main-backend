using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Subscriptions.CreateSubscriptions;

public sealed record CreateSubscriptionCommand(
    Guid PlanId,
    Guid OrganizationId,
    string SubscriptionType,
    DateTime PeriodStart) : ICommand<Guid>;

