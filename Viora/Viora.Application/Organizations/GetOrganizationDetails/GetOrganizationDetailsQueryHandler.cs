using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;

namespace Viora.Application.Organizations.GetOrganizationDetails;

internal class GetOrganizationDetailsQueryHandler(
    IOrganizationRepository organizationRepository,
    IReadOnlyList<Country> countries
    ) : IQueryHandler<GetOrganizationDetailsQuery, OrganizationDetailsResponse>
{
    public async Task<Result<OrganizationDetailsResponse>> Handle(GetOrganizationDetailsQuery request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with ID {request.OrganizationId} not found.");

        //TODO get branches from the repo
        var country = countries.First(c => c.Id == organization.CountryId);

        var response = new OrganizationDetailsResponse(
            organization.Id,
            organization.Name,
            country.Name,
            country.IsoAlphaThree,
            organization.ServiceSpecification.Type.ToString(),
            organization.ServiceSpecification.Description,
            organization.SupportEmail,
            organization.JoinedOnUtc,
            []);

        return Result.Success(response);
    }
}