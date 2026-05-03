using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.DTO;

namespace Viora.Application.Plans.GetPlanById;

public sealed record GetPlanByIdQuery(Guid Id) : IQuery<PlanDTO>;
