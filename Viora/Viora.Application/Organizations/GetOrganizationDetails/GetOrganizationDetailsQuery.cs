using Viora.Application.Abstractions.Messaging;
using Viora.Application.Organizations.Shared;

namespace Viora.Application.Organizations.GetOrganizationDetails;

public record GetOrganizationDetailsQuery(Guid OrganizationId) : IQuery<OrganizationDetailsResponse>;