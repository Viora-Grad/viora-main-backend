using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;

namespace Viora.Application.Branches.UpdateSchedule;

internal sealed class UpdateScheduleCommandHandler(IBranchRepository branchRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateScheduleCommand>
{
    public async Task<Result> Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with Id {request.BranchId} does not exist");

        foreach (var record in request.Schedule)
        {
            var result = branch.SetBusinessHours(record.Day, record.OpenTime, record.CloseTime);
            if (result.IsFailure)
                return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
