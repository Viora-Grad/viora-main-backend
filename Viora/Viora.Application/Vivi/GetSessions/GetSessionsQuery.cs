using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Vivi.Shared.Internals;

namespace Viora.Application.Vivi.GetSessions;

public sealed record GetSessionsQuery(Guid UserId, Persona Peronsa, int PageNumber, int PageSize) : IQuery<PaginatedModel<GetSessionsResponse>>;