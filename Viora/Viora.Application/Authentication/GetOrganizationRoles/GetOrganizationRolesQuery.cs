using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Authentication.GetOrganizationRoles;

public sealed record GetOrganizationRolesQuery(Guid OrganizationId) : IQuery<IReadOnlyCollection<GetRolesResponse>>;
