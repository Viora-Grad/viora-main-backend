using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Staffs.AssignRoles;

public class AssignRolesCommandHandler(
    IRoleRepository roleRepository,
    IStaffRepository staffRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AssignRolesCommand>
{
    public async Task<Result> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken) ??
            throw new NotFoundException("Staff not found");
        var orgRoles = await roleRepository.GetOrganizationRolesAsync(staff.OrganizationId, cancellationToken);

        var rolesToAssign = orgRoles.Where(r => request.RoleIds.Contains(r.Id)).ToList();
        if (!rolesToAssign.Any() && rolesToAssign.Count != request.RoleIds.Count)
        {
            throw new NotFoundException("One or more roles not found");
        }
        staff.AddRoles(rolesToAssign);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
