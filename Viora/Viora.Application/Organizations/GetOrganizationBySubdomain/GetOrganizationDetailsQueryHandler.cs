using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Organizations.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;

namespace Viora.Application.Organizations.GetOrganizationBySubdomain;

internal class GetOrganizationDetailsBySubdomainQueryHandler(
    IOrganizationRepository organizationRepository,
    IBranchRepository branchRepository,
    IReadOnlyList<Country> countries
    ) : IQueryHandler<GetOrganizationDetailsBySubdomainQuery, OrganizationDetailsResponse>
{
    public async Task<Result<OrganizationDetailsResponse>> Handle(GetOrganizationDetailsBySubdomainQuery request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetBySubdomainAsync(request.Subdomain, cancellationToken)
            ?? throw new NotFoundException($"Organization with subdomain {request.Subdomain} not found.");

        var country = countries.First(c => c.Id == organization.CountryId);

        var branches = await branchRepository.GetByOrganizationIdAsync(organization.Id, cancellationToken);

        var response = new OrganizationDetailsResponse(
            organization.Id,
            organization.Name,
            organization.About,
            country.Name,
            country.IsoAlphaThree,
            organization.ServicesProvided.Select(s => s.Value),
            organization.ServiceDescription,
            organization.SupportEmail,
            organization.JoinedOnUtc,
            branches.Select(x => new MinimalBranch(x.Id, x.Gallery.Count == 0 ? null : x.Gallery.First().Id, x.Address.Value, x.OpenedAtUtc)),
            organization.SubDomain);

        return Result.Success(response);
    }
}