using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;

namespace Viora.Application.Staffs.DeleteStaff;

public class DeleteStaffCommandHandler(
    IUserContext context,
    IStaffRepository staffRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock
    ) : ICommandHandler<DeleteStaffCommand>
{
    public async Task<Result> Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(request.Id, cancellationToken);
        if (staff == null) return Result.Success();

        if (staff.OrganizationId != context.OrganizationId)
        {
            throw new UnauthorizedAccessException();
        }
        staff.Delete(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

