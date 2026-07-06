namespace Viora.Application.Feedbacks.GetFeedbacks;

public sealed record GetFeedbacksResponse(
    Guid Id,
    Guid BranchId,
    Guid UserId,
    string UserName,
    int ServiceRatingOutOfTen,
    int BranchOutOfTen,
    int SystemExperienceOutOfTen,
    double TotalRatingOurOfTen,
    DateTime CreatedAtUtc,
    DateTime? EditedAtUtc,
    string? Comment);
