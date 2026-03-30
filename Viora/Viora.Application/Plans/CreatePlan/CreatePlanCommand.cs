using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Plans.CreatePlan;

public sealed record CreatePlanCommand(
    string PlanCode,
    string PlanName,
    string PlanDescription,
    int Version,
    string Content,
    string ContentForm) : ICommand<Guid>;
