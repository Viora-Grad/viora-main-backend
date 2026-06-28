using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.GetOrganizationDetails;

public record GetOrganizationDetailsQuery(Guid OrganizationId) : IQuery<OrganizationDetailsResponse>;