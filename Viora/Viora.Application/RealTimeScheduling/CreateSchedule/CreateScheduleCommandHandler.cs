using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Application.RealTimeScheduling.CreateSchedule;

public class CreateScheduleCommandHandler(
    //IBranchRepository branchRepository
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<CreateScheduleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {/*
        var branch = await branchRepository.GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {request.BranchId} not found");*/
        var day = Enum.Parse<DayOfWeek>(request.DayOfWeek);

        var branchSchedule = await scheduleRepository.getByBranchIdAndDayAsync(request.BranchId, day, cancellationToken);

        if (branchSchedule is not null)
            return Result.Failure<Guid>(ScheduleError.ScheduleOverLap);

        var newSchedule = Schedule.Create(request.BranchId, day);
        scheduleRepository.Add(newSchedule);
        await unitOfWork.SaveChangesAsync();

        return Result.Success(newSchedule.Id);
    }
}
