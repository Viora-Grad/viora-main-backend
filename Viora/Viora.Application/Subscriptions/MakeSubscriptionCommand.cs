using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Subscriptions;

public sealed record MakeSubscriptionCommand(
    Guid PlanId,
    Guid OrganizationId,
    string SubscriptionType,
    DateTime PeriodStart) : ICommand<Guid>;

