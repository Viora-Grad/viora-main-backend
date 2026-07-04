using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;

namespace Viora.Application.Staffs.ChangeStatus;

internal class ChangeStatusCommandHandler(
    IUserContext context,
    IStaffRepository staffRepository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<ChangeStatusCommand>
{
    public async Task<Result> Handle(ChangeStatusCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new NotFoundException($"Staff with ID : {request.Id} not found.");

        var orgId = context.OrganizationId;
        if (staff.OrganizationId != orgId)
        {
            throw new UnauthorizedAccessException();
        }
        var status = Enum.TryParse<StaffStatus>(request.Status, true, out var parsedStatus);
        if (!status)
        {
            throw new ArgumentException("Invalid staff status.");
        }
        switch (parsedStatus)
        {
            case StaffStatus.Active:
                staff.Activate();
                break;
            case StaffStatus.Suspended:
                staff.Suspend();
                break;
            default:
                throw new ArgumentException("Cannot change staff status.");
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
