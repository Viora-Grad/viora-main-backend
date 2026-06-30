using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Authentication.GetOrganizationRoles;

internal class GetOrganizationRolesQueryHandler(
    IRoleRepository repository) : IQueryHandler<GetOrganizationRolesQuery, IReadOnlyCollection<GetRolesResponse>>
{
    public async Task<Result<IReadOnlyCollection<GetRolesResponse>>> Handle(GetOrganizationRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await repository.GetOrganizationRolesAsync(request.OrganizationId, cancellationToken);
        IReadOnlyCollection<GetRolesResponse> response = roles.Select(role => new GetRolesResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description!
        }).ToList().AsReadOnly();
        return Result.Success(response);
    }
}
