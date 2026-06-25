
using NetTopologySuite.Geometries;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Feedbacks;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;

namespace Viora.Application.Branches.SearchBranches;

internal sealed class SearchBranchesQueryHandler(
    IBranchRepository branchRepository,
    IOrganizationRepository organizationRepository,
    IFeedbackRepository feedbackRepository,
    IDateTimeProvider dateTimeProvider) : IQueryHandler<SearchBranchesQuery, PaginatedModel<SearchBranchesResponse>>
{
    public async Task<Result<PaginatedModel<SearchBranchesResponse>>> Handle(
        SearchBranchesQuery request, CancellationToken cancellationToken)
    {
        Point? searchPoint = request.Latitude.HasValue && request.Longitude.HasValue
            ? new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 }
            : null;

        var services = request.ServicesFilter?
            .Select(ServiceType.FromValue)
            .ToList()
            .AsReadOnly();

        var parameters = new BranchSearchParameters(
            request.BranchId,
            request.OrganizationId,
            request.Status,
            services,
            searchPoint,
            request.DistanceWithinMeters,
            request.OrderBy,
            request.Page,
            request.PageSize);

        var spec = new BranchSearchSpecification(parameters);
        var branches = await branchRepository.ListAsync(spec, cancellationToken);

        if (!branches.Any())
            return Result.Success(PaginatedModel<SearchBranchesResponse>.Empty(request.Page, request.PageSize));

        var branchIds = branches.Select(b => b.Id).ToList();
        var orgIds = branches.Select(b => b.OrganizationId).Distinct().ToList();

        // Awaited sequentially: both repositories share the same scoped DbContext, which does not
        // allow concurrent operations (Task.WhenAll over them throws a DbContext concurrency error).
        var ratingsDict = await feedbackRepository.GetAverageRatingsByBranchIdsAsync(branchIds, cancellationToken);
        var orgDict = (await organizationRepository.ListAsync(
            new OrganizationByIdsSpecification(orgIds), cancellationToken))
            .ToDictionary(o => o.Id, o => o.Name.Value);

        var utcNow = dateTimeProvider.UtcNow;
        var orderByRating = request.OrderBy?.Any(o => o.Equals("rating", StringComparison.OrdinalIgnoreCase)) ?? false;

        IEnumerable<SearchBranchesResponse> response = TransformToResponse(branches, ratingsDict, orgDict, utcNow);

        // apply post-query filters and ordering that can't be done at DB level
        IEnumerable<SearchBranchesResponse> filtered = response;

        if (request.IsCurrentlyOpen.HasValue)
            filtered = filtered.Where(r => r.IsOpen == request.IsCurrentlyOpen.Value);

        if (request.MinimumRating > 0)
            filtered = filtered.Where(r => r.Rating >= request.MinimumRating);

        if (orderByRating)
            filtered = filtered.OrderByDescending(r => r.Rating);

        var totalCount = await branchRepository.CountAsync(spec, cancellationToken);
        return Result.Success(new PaginatedModel<SearchBranchesResponse>(
            filtered, request.Page, request.PageSize, totalCount));
    }

    private static IEnumerable<SearchBranchesResponse> TransformToResponse(IReadOnlyList<Branch> branches, Dictionary<Guid, double> ratingsDict, Dictionary<Guid, string> orgDict, DateTime utcNow)
    {
        return branches.Select(b =>
             new SearchBranchesResponse(
                b.Id,
                b.OrganizationId,
                orgDict.GetValueOrDefault(b.OrganizationId, string.Empty),
                b.IsCurrentlyOpen(utcNow),
                b.OpenedAtUtc,
                (float)ratingsDict.GetValueOrDefault(b.Id, 0.0),
                b.Status,
                b.Address.Value,
                b.Gallery
                    .Select(g => new MediaResponse(g.Id, g.MimeType, g.Name, g.UploadedAtUtc))
                    .FirstOrDefault(),
                string.Empty,
                new(b.Location))
        );
    }
}