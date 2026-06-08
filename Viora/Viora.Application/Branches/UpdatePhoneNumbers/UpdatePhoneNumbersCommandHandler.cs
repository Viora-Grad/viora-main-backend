using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Shared.Internal;

namespace Viora.Application.Branches.UpdatePhoneNumbers;

internal sealed class UpdatePhoneNumbersCommandHandler(
    IBranchRepository branchRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdatePhoneNumbersCommand>
{
    public async Task<Result> Handle(UpdatePhoneNumbersCommand request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {request.BranchId} not found.");

        branch.UpdatePhoneNumbers(request.PhoneNumbers.Select(p => new PhoneNumber(p)));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
