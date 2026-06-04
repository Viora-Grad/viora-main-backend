using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;

namespace Viora.Application.RealTimeScheduling.GetBranchSchedule;

public record GetBranchScheduleQuery(
    Guid BranchId) : IQuery<List<BranchScheduleResponse>>;
