using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Authentication.CreateStaffRole;

internal class CreateStaffRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateStaffRoleCommand, int>
{
    public async Task<Result<int>> Handle(CreateStaffRoleCommand request, CancellationToken cancellationToken)
    {
        var permissions = Permission.All
            .Where(p => request.PermissionsIds.Contains(p.Id))
            .ToList();

        if (!permissions.Any() || permissions.Count != request.PermissionsIds.Count)
            throw new NotFoundException("One or more specified permissions were not found.");

        var role = new Role(request.RoleName, request.RoleDescription, request.OrganizationId);
        roleRepository.Add(role);
        roleRepository.AttachRange(permissions);

        foreach (var permission in permissions)
        {
            role.Permissions.Add(permission);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(role.Id);
    }
}
