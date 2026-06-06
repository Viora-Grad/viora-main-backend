using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;

namespace Viora.Application.Organizations.SearchApplications;

public record SearchApplicationsQuery(Guid? Id, Guid? OwnerId, string? Status, string? ReferralSource, int Page, int PageSize) : IQuery<PaginatedModel<ApplicationsResponse>>;