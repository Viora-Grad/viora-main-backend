
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Feedbacks.AddFeedback;

public sealed record AddFeedbackCommand(
    Guid BranchId,
    Guid UserId,
    int ServiceRatingOutOfTen,
    int BranchOutOfTen,
    int SystemExperienceOutOfTen,
    string? Comment) : ICommand<Guid>;