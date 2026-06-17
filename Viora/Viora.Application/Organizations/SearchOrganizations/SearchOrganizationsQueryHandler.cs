using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;

namespace Viora.Application.Organizations.SearchOrganizations;

internal class SearchOrganizationsQueryHandler(
    IOrganizationRepository organizationRepository,
    IReadOnlyList<Country> countries) : IQueryHandler<SearchOrganizationsQuery, PaginatedModel<OrganizationsResponse>>
{
    public async Task<Result<PaginatedModel<OrganizationsResponse>>> Handle(SearchOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var country = countries.FirstOrDefault(c =>
            c.Name.Equals(request.Country, StringComparison.OrdinalIgnoreCase));

        ServiceType? serviceType = request.ServiceType != null ? ServiceType.FromValue(request.ServiceType) : null;

        OrganizationSearchParameters specificationParameters = new(
            request.Id,
            country?.Id,
            request.Name,
            request.MinimumRating,
            serviceType,
            request.Status,
            request.SortyBy,
            request.Page,
            request.PageSize
            );
        var specfication = new OrganizationSearchSpecification(specificationParameters);

        var organizations = await organizationRepository.ListAsync(specfication, cancellationToken);

        if (!organizations.Any())
            return Result.Success(PaginatedModel<OrganizationsResponse>.Empty(request.Page, request.PageSize));

        var response = organizations.Select(org =>
            new OrganizationsResponse(
            org.Id,
            org.LogoId,
            org.Name,
            countries.First(c => c.Id == org.CountryId).Name,
            org.ServiceDescription,
            org.ServicesProvided.Select(s => s.Value),
            org.Rating.Count,
            org.Rating.AverageOutOfTen)
        );

        var countAll = await organizationRepository.CountAsync(specfication, cancellationToken);
        PaginatedModel<OrganizationsResponse> paginatedModel = new([.. response], request.Page, request.PageSize, countAll);

        return Result.Success(paginatedModel);

    }
}
