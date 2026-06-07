using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.Shared;

namespace Viora.Application.Plans.GetPlanById;

public sealed record GetPlanByIdQuery(Guid Id) : IQuery<PlanResponse>;
