using Viora.Application.Abstractions.Messaging;
using Viora.Application.Organizations.Shared;

namespace Viora.Application.Organizations.GetOrganizationBySubdomain;

public record GetOrganizationDetailsBySubdomainQuery(string Subdomain) : IQuery<OrganizationDetailsResponse>;