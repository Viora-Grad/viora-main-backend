using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;

namespace Viora.Application.Organizations.GetOrganizationDetails;

internal class GetOrganizationDetailsQueryHandler(
    IOrganizationRepository organizationRepository,
    IBranchRepository branchRepository,
    IReadOnlyList<Country> countries
    ) : IQueryHandler<GetOrganizationDetailsQuery, OrganizationDetailsResponse>
{
    public async Task<Result<OrganizationDetailsResponse>> Handle(GetOrganizationDetailsQuery request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with ID {request.OrganizationId} not found.");

        var country = countries.First(c => c.Id == organization.CountryId);

        var branches = await branchRepository.GetByOrganizationIdAsync(request.OrganizationId, cancellationToken);

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
            branches.Select(x => new MinimalBranch(x.Id, x.Gallery.Count == 0 ? null : x.Gallery.First().Id, x.Address.Value, x.OpenedAtUtc)));

        return Result.Success(response);
    }
}