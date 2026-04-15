using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.DTO;

namespace Viora.Application.Plans.GetPlans;

public sealed record GetPlansQuery : IQuery<List<PlanDTO>>;
