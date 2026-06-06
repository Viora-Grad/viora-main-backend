using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;
using Viora.Domain.Vivi.ChatSessions;

namespace Viora.Application.Vivi.GetSessions;

internal class GetSessionsQueryHandler(IChatSessionRepository chatSessionRepository) : IQueryHandler<GetSessionsQuery, PaginatedModel<GetSessionsResponse>>
{
    public async Task<Result<PaginatedModel<GetSessionsResponse>>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var result = await chatSessionRepository.GetSessionsByUserIdAsync(request.UserId, request.Peronsa, request.PageNumber, request.PageSize, cancellationToken);
        var countMatching = await chatSessionRepository.GetCountSessionsByUserIdAsync(request.UserId, request.Peronsa, cancellationToken);

        var response = result.Select(r => new GetSessionsResponse(r.Id, r.Name, r.LatestActivityUtc));
        var paginatedResult = new PaginatedModel<GetSessionsResponse>(response, request.PageNumber, request.PageSize, countMatching);

        return Result.Success(paginatedResult);
    }
}
