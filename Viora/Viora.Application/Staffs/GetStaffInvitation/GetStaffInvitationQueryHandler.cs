using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;

namespace Viora.Application.Staffs.GetStaffInvitation;

internal class GetStaffInvitationQueryHandler(
    IStaffTokenRepository repository) : IQueryHandler<GetStaffInvitationQuery, StaffToken>
{
    public async Task<Result<StaffToken>> Handle(GetStaffInvitationQuery request, CancellationToken cancellationToken)
    {
        var token = await repository.GetByIdAsync(request.InvitationId, cancellationToken) ??
            throw new NotFoundException("invitation not found");

        return Result.Success(token);

    }
}
