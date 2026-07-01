using Viora.Application.Abstractions.Messaging;
using Viora.Application.Organizations.Shared;

namespace Viora.Application.Organizations.GetMyOrganization;

public record GetMyOrganizationDetailsQuery(Guid OwnerId) : IQuery<OrganizationDetailsResponse>;