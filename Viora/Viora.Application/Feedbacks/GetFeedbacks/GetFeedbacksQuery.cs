using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;

namespace Viora.Application.Feedbacks.GetFeedbacks;

public sealed record GetFeedbacksQuery(
    Guid? BranchId,
    Guid? UserId,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedModel<GetFeedbacksResponse>>;