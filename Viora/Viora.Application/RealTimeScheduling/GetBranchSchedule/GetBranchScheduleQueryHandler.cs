using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.RealTimeScheduling.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Application.RealTimeScheduling.GetBranchSchedule;

public class GetBranchScheduleQueryHandler(
    IBranchRepository branchRepository,
    IScheduleRepository scheduleRepository) : IQueryHandler<GetBranchScheduleQuery, List<BranchScheduleResponse>>
{
    public async Task<Result<List<BranchScheduleResponse>>> Handle(GetBranchScheduleQuery request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with Id {request.BranchId} not found");

        var branchSchedules = await scheduleRepository.getByBranchIdAsync(request.BranchId, cancellationToken);

        if (branchSchedules is null || !branchSchedules.Any())
            return Result.Failure<List<BranchScheduleResponse>>(ScheduleError.ScheduleOverLap);

        var schedules = BranchScheduleResponse.MapToList(branchSchedules);

        return Result.Success(schedules);
    }
}
