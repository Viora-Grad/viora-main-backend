using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Enums;

namespace Viora.Application.Organizations.SearchOrganizations;

internal class SearchOrganizationsQueryHandler(
    IOrganizationRepository organizationRepository,
    IMediaRepository mediaRepository,
    IStorageService storageService,
    IReadOnlyList<Country> countries) : IQueryHandler<SearchOrganizationsQuery, PaginatedModel<OrganizationsResponse>>
{

    public async Task<Result<PaginatedModel<OrganizationsResponse>>> Handle(SearchOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var country = countries.FirstOrDefault(c =>
            c.Name.Equals(request.Country, StringComparison.OrdinalIgnoreCase));

        ServiceType? serviceType = request.ServiceType != null ? Enum.Parse<ServiceType>(request.ServiceType, ignoreCase: true) : null;

        OrganizationSearchParameters specificationParameters = new(
            request.Id,
            country?.Id,
            request.Name,
            request.MinimumRating,
            serviceType,
            null,
            request.Page,
            request.PageSize
            );
        var specfication = new OrganizationSearchSpecification(specificationParameters);

        var organizations = await organizationRepository.ListAsync(specfication, cancellationToken);

        if (!organizations.Any())
            return Result.Success(PaginatedModel<OrganizationsResponse>.Empty(request.Page, request.PageSize));

        var logoIds = organizations
            .Select(org => org.LogoId)
            .OfType<Guid>()
            .Distinct()
            .ToList();

        var logos = await mediaRepository.GetByIdsAsync(logoIds, cancellationToken);

        // Convert to Dictionary for O(1) lookup speed during mapping
        var logoDict = logos.ToDictionary(m => m.Id);

        var responseTasks = organizations.Select(async org =>
        {
            MediaFile? logo = null;
            if (org.LogoId.HasValue)
                logoDict.TryGetValue(org.LogoId.Value, out logo);

            MediaResponse? logoResponse = null;
            if (logo is not null)
            {
                await using var stream = await storageService.GetFileStreamAsync(logo.Key);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, cancellationToken);
                logoResponse = new MediaResponse(
                    Convert.ToBase64String(ms.ToArray()),
                    logo.MimeType,
                    logo.Name);
            }

            return new OrganizationsResponse(
                org.Id,
                logoResponse!,
                org.Name,
                countries.First(c => c.Id == org.CountryId).Name,
                org.ServiceDescription,
                org.ServicesProvided.Select(s => s.ToString()),
                org.Rating.Count,
                org.Rating.AverageOutOfTen);
        });

        var response = await Task.WhenAll(responseTasks);

        var countAll = await organizationRepository.CountAsync(specfication, cancellationToken);
        PaginatedModel<OrganizationsResponse> paginatedModel = new([.. response], request.Page, request.PageSize, countAll);

        return Result.Success(paginatedModel);

    }
}
