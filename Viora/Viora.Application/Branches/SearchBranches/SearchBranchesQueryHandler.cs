using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;

namespace Viora.Application.Branches.SearchBranches;

internal sealed class SearchBranchesQueryHandler : IQueryHandler<SearchBranchesQuery, PaginatedModel<SearchBranchesResponse>>
{
    public Task<Result<PaginatedModel<SearchBranchesResponse>>> Handle(SearchBranchesQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
