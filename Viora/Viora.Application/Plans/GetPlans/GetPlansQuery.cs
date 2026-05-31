using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.Shared;

namespace Viora.Application.Plans.GetPlans;

public sealed record GetPlansQuery : IQuery<List<PlanResponse>>;
