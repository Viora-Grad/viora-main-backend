namespace Viora.Api.Controllers.Feedbacks;

public sealed record AddFeedbackRequest(
    Guid BranchId,
    int ServiceRatingOutOfTen,
    int BranchOutOfTen,
    int SystemExperienceOutOfTen,
    string? Comment);

public sealed record UpdateFeedbackRequest(
    int ServiceRatingOutOfTen,
    int BranchOutOfTen,
    int SystemExperienceOutOfTen,
    string? Comment);
