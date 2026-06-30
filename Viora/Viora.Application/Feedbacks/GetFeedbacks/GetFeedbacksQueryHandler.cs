using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;
using Viora.Domain.Feedbacks;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Feedbacks.GetFeedbacks;

internal sealed class GetFeedbacksQueryHandler(
    IFeedbackRepository feedbackRepository,
    IUserRepository userRepository) : IQueryHandler<GetFeedbacksQuery, PaginatedModel<GetFeedbacksResponse>>
{
    public async Task<Result<PaginatedModel<GetFeedbacksResponse>>> Handle(GetFeedbacksQuery request, CancellationToken cancellationToken)
    {
        var (feedbacks, totalCount) = await feedbackRepository.GetPagedAsync(
            request.BranchId,
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var names = await userRepository.GetNamesDictAsync(
            feedbacks.Select(f => f.UserId),
            cancellationToken);

        var items = feedbacks.Select(f => new GetFeedbacksResponse(
            f.BranchId,
            f.UserId,
            names.TryGetValue(f.UserId, out var name) ? name : string.Empty,
            f.Ratings.ServiceOutOfTen,
            f.Ratings.BranchOutOfTen,
            f.Ratings.SystemOutOfTen,
            (f.Ratings.ServiceOutOfTen + f.Ratings.BranchOutOfTen + f.Ratings.SystemOutOfTen) / 3.0,
            f.SubmittedOnUtc,
            f.EditedOnUtc,
            f.Comment?.Value)).ToList();

        return Result.Success(new PaginatedModel<GetFeedbacksResponse>(items, request.Page, request.PageSize, totalCount));
    }
}
