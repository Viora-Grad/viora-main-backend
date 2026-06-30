using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Feedbacks.UpdateFeedback;

public sealed record UpdateFeedbackCommand(
    Guid FeedbackId,
    Guid UserId,
    int ServiceRatingOutOfTen,
    int BranchOutOfTen,
    int SystemExperienceOutOfTen,
    string? Comment) : ICommand;