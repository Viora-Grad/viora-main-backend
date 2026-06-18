using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;

namespace Viora.Application.Branches.UpdateBranchStatus;

internal sealed class UpdateBranchStatusCommandHandler(IBranchRepository branchRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateBranchStatusCommand>
{
    public async Task<Result> Handle(UpdateBranchStatusCommand request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {request.Id} not found");

        branch.UpdateStatus(request.Status);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
